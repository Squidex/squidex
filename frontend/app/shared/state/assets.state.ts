/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { compareStrings, DialogService, ErrorDto, getPagingInfo, ListState, MathHelper, shareSubscribed, State, StateSynchronizer } from '@app/framework';
import { EMPTY, forkJoin, Observable, of, throwError } from 'rxjs';
import { catchError, finalize, switchMap, tap } from 'rxjs/operators';
import { AnnotateAssetDto, AssetDto, AssetFolderDto, AssetsService, RenameAssetFolderDto } from './../services/assets.service';
import { AppsState } from './apps.state';
import { Query, QueryFullTextSynchronizer } from './query';

export type AssetPathItem = { id: string, folderName: string };

export type TagsAvailable = { [name: string]: number };
export type TagsSelected = { [name: string]: boolean };
export type Tag = { name: string, count: number; };

const EMPTY_FOLDERS: { canCreate: boolean, items: ReadonlyArray<AssetFolderDto>, path?: ReadonlyArray<AssetFolderDto> } = { canCreate: false, items: [] };

export const ROOT_ITEM: AssetPathItem = { id: MathHelper.EMPTY_GUID, folderName: 'i18n:assets.specialFolder.root' };

interface Snapshot extends ListState<Query> {
    // The current assets.
    assets: ReadonlyArray<AssetDto>;

    // All assets tags.
    tagsAvailable: TagsAvailable;

    // The selected asset tags.
    tagsSelected: TagsSelected;

    // The current asset folders.
    folders: ReadonlyArray<AssetFolderDto>;

    // The folder path.
    path: ReadonlyArray<AssetPathItem>;

    // The parent folder.
    parentId: string;

    // Indicates if the assets are loaded once.
    isLoadedOnce?: boolean;

    // Indicates if the user can create assets.
    canCreate?: boolean;

    // Indicates if the user can create asset folders.
    canCreateFolders?: boolean;
}

@Injectable()
export class AssetsState extends State<Snapshot> {
    public tagsUnsorted =
        this.project(x => x.tagsAvailable);

    public tagsSelected =
        this.project(x => x.tagsSelected);

    public tags =
        this.projectFrom(this.tagsUnsorted, x => sort(x));

    public tagsNames =
        this.projectFrom(this.tagsUnsorted, x => Object.keys(x));

    public selectedTagNames =
        this.projectFrom(this.tagsSelected, x => Object.keys(x));

    public assets =
        this.project(x => x.assets);

    public paging =
        this.project(x => getPagingInfo(x, x.assets.length));

    public query =
        this.project(x => x.query);

    public folders =
        this.project(x => x.folders);

    public hasFolders =
        this.project(x => x.folders.length > 0);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public path =
        this.project(x => x.path);

    public pathAvailable =
        this.project(x => x.path.length > 0);

    public parentFolder =
        this.project(x => getParent(x.path));

    public canCreate =
        this.project(x => x.canCreate === true);

    public canCreateFolders =
        this.project(x => x.canCreateFolders === true);

    constructor(
        private readonly appsState: AppsState,
        private readonly assetsService: AssetsService,
        private readonly dialogs: DialogService
    ) {
        super({
            folders: [],
            assets: [],
            page: 0,
            pageSize: 30,
            parentId: ROOT_ITEM.id,
            path: [ROOT_ITEM],
            tagsAvailable: {},
            tagsSelected: {},
            total: 0
        });
    }

    public loadAndListen(synchronizer: StateSynchronizer) {
        synchronizer.mapTo(this)
            .withPaging('assets', 30)
            .withString('parentId', 'parent')
            .withStrings('tagsSelected', 'tags')
            .withSynchronizer(QueryFullTextSynchronizer.INSTANCE)
            .whenSynced(() => this.loadInternal(false))
            .build();
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload: boolean): Observable<any> {
        this.next({ isLoading: true });

        const { page, pageSize, query, tagsSelected } =  this.snapshot;

        const q: any = { take: pageSize, skip: pageSize * page };

        const hasQuery = !!query?.fullText || Object.keys(tagsSelected).length > 0;

        if (hasQuery) {
            if (query) {
                q.query = query;
            }

            const searchTags = Object.keys(this.snapshot.tagsSelected);

            if (searchTags.length > 0) {
                q.tags = searchTags;
            }
        } else {
            q.parentId = this.snapshot.parentId;
        }

        const assets$ =
            this.assetsService.getAssets(this.appName, q);

        const assetFolders$ =
            !hasQuery ?
                this.assetsService.getAssetFolders(this.appName, this.snapshot.parentId) :
                of(EMPTY_FOLDERS);

        const tags$ =
            !hasQuery || !this.snapshot.isLoadedOnce ?
                this.assetsService.getTags(this.appName) :
                of(this.snapshot.tagsAvailable);

        return forkJoin(([assets$, assetFolders$, tags$])).pipe(
            tap(([assetsResult, assetFolders, tagsAvailable]) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:assets.reloaded');
                }

                const path = assetFolders.path ?
                    [ROOT_ITEM, ...assetFolders.path] :
                    [];

                const { items: assets, total } = assetsResult;

                this.next(s => ({
                    ...s,
                    assets,
                    folders: assetFolders.items,
                    canCreate: assetsResult.canCreate,
                    canCreateFolders: assetFolders.canCreate,
                    isLoaded: true,
                    isLoadedOnce: true,
                    isLoading: false,
                    path,
                    tagsAvailable,
                    total
                }));
            }),
            finalize(() => {
                this.next({ isLoading: false });
            }),
            shareSubscribed(this.dialogs));
    }

    public addAsset(asset: AssetDto) {
        if (asset.parentId !== this.snapshot.parentId || this.snapshot.assets.find(x => x.id === asset.id)) {
            return;
        }

        this.next(s => {
            const assets = [asset, ...s.assets].slice(0, s.pageSize);

            const tags = updateTags(s, asset);

            return { ...s, assets, total: s.total + 1, ...tags };
        });
    }

    public createAssetFolder(folderName: string) {
        return this.assetsService.postAssetFolder(this.appName, { folderName, parentId: this.snapshot.parentId }).pipe(
            tap(assetFolder => {
                if (assetFolder.parentId !== this.snapshot.parentId) {
                    return;
                }

                this.next(s => {
                    const assetFolders = [...s.folders, assetFolder].sortedByString(x => x.folderName);

                    return { ...s, folders: assetFolders };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public updateAsset(asset: AssetDto, request: AnnotateAssetDto) {
        return this.assetsService.putAsset(this.appName, asset, request, asset.version).pipe(
            tap(updated => {
                this.next(s => {
                    const tags = updateTags(s, updated);

                    const assets = s.assets.replaceBy('id', updated);

                    return { ...s, assets, ...tags };
                });
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public updateAssetFolder(assetFolder: AssetFolderDto, request: RenameAssetFolderDto) {
        return this.assetsService.putAssetFolder(this.appName, assetFolder, request, assetFolder.version).pipe(
            tap(updated => {
                this.next(s => {
                    const assetFolders = s.folders.replaceBy('id', updated);

                    return { ...s, folders: assetFolders };
                });
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public moveAsset(asset: AssetDto, parentId?: string) {
        if (asset.parentId === parentId) {
            return EMPTY;
        }

        this.next(s => {
            const assets = s.assets.filter(x => x.id !== asset.id);

            return { ...s, assets };
        });

        return this.assetsService.putAssetItemParent(this.appName, asset, { parentId }, asset.version).pipe(
            catchError(error => {
                this.next(s => {
                    const assets = [asset, ...s.assets];

                    return { ...s, assets };
                });

                return throwError(error);
            }),
            shareSubscribed(this.dialogs));
    }

    public moveAssetFolder(assetFolder: AssetFolderDto, parentId?: string) {
        if (assetFolder.id === parentId || assetFolder.parentId === parentId) {
            return EMPTY;
        }

        this.next(s => {
            const assetFolders = s.folders.filter(x => x.id !== assetFolder.id);

            return { ...s, folders: assetFolders };
        });

        return this.assetsService.putAssetItemParent(this.appName, assetFolder, { parentId }, assetFolder.version).pipe(
            catchError(error => {
                this.next(s => {
                    const assetFolders = [...s.folders, assetFolder].sortedByString(x => x.folderName);

                    return { ...s, folders: assetFolders };
                });

                return throwError(error);
            }),
            shareSubscribed(this.dialogs));
    }

    public deleteAsset(asset: AssetDto) {
        return this.assetsService.deleteAssetItem(this.appName, asset, true, asset.version).pipe(
            catchError((error: ErrorDto) => {
                if (error.statusCode === 400) {
                    return this.dialogs.confirm(
                        'i18n:assets.deleteReferrerConfirmTitle',
                        'i18n:assets.deleteReferrerConfirmText',
                        'deleteReferencedAsset'
                    ).pipe(
                        switchMap(confirmed => {
                            if (confirmed) {
                                return this.assetsService.deleteAssetItem(this.appName, asset, false, asset.version);
                            } else {
                                return EMPTY;
                            }
                        })
                    );
                } else {
                    return throwError(error);
                }
            }),
            tap(() => {
                this.next(s => {
                    const assets = s.assets.filter(x => x.id !== asset.id);

                    const tags = updateTags(s, undefined, asset);

                    return { ...s, assets, total: s.total - 1, ...tags };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public deleteAssetFolder(assetFolder: AssetFolderDto): Observable<any> {
        return this.assetsService.deleteAssetItem(this.appName, assetFolder, false, assetFolder.version).pipe(
            tap(() => {
                this.next(s => {
                    const assetFolders = s.folders.filter(x => x.id !== assetFolder.id);

                    return { ...s, folders: assetFolders };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public navigate(parentId: string) {
        this.next({ parentId, query: undefined, tagsSelected: {} });

        return this.loadInternal(false);
    }

    public page(paging: { page: number, pageSize: number }) {
        this.next(paging);

        return this.loadInternal(false);
    }

    public toggleTag(tag: string): Observable<any> {
        const tagsSelected = { ...this.snapshot.tagsSelected };

        if (tagsSelected[tag]) {
            delete tagsSelected[tag];
        } else {
            tagsSelected[tag] = true;
        }

        return this.searchInternal(null, tagsSelected);
    }

    public selectTags(tags: ReadonlyArray<string>): Observable<any> {
        const tagsSelected = {};

        for (const tag of tags) {
            tagsSelected[tag] = true;
        }

        return this.searchInternal(null, tagsSelected);
    }

    public resetTags(): Observable<any> {
        return this.searchInternal(null, {});
    }

    public search(query?: Query): Observable<any> {
        return this.searchInternal(query);
    }

    private searchInternal(query?: Query | null, tags?: TagsSelected) {
        this.next(s => {
            const newState = { ...s, page: 0 };

            if (query !== null) {
                newState.query = query;
            }

            if (tags) {
                newState.tagsSelected = tags;
            }

            return newState;
        });

        return this.loadInternal(false);
    }

    public get parentId() {
        return this.snapshot.parentId;
    }

    private get appName() {
        return this.appsState.appName;
    }
}

function updateTags(snapshot: Snapshot, newAsset?: AssetDto, oldAsset?: AssetDto) {
    if (!oldAsset && newAsset) {
        oldAsset = snapshot.assets.find(x => x.id === newAsset.id);
    }

    const tagsAvailable = { ...snapshot.tagsAvailable };
    const tagsSelected = { ...snapshot.tagsSelected };

    if (oldAsset) {
        for (const tag of oldAsset.tags) {
            if (tagsAvailable[tag] === 1) {
                delete tagsAvailable[tag];
                delete tagsSelected[tag];
            } else {
                tagsAvailable[tag]--;
            }
        }
    }

    if (newAsset) {
        for (const tag of newAsset.tags) {
            if (tagsAvailable[tag]) {
                tagsAvailable[tag]++;
            } else {
                tagsAvailable[tag] = 1;
            }
        }
    }

    return { tagsAvailable, tagsSelected };
}

function sort(tags: { [name: string]: number }) {
    return Object.keys(tags).sort(compareStrings).map(name => ({ name, count: tags[name] }));
}

function getParent(path: ReadonlyArray<AssetPathItem>) {
    return path.length > 1 ? { folderName: 'i18n:assets.specialFolder.parent', id: path[path.length - 2].id } : undefined;
}

@Injectable()
export class AssetsDialogState extends AssetsState { }