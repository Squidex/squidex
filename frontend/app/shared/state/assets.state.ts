/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { compareStrings, DialogService, ErrorDto, MathHelper, Pager, shareSubscribed, State, StateSynchronizer } from '@app/framework';
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

interface Snapshot {
    // All assets tags.
    tagsAvailable: TagsAvailable;

    // The selected asset tags.
    tagsSelected: TagsSelected;

    // The current assets.
    assets: ReadonlyArray<AssetDto>;

    // The current asset folders.
    assetFolders: ReadonlyArray<AssetFolderDto>;

    // The pagination information.
    assetsPager: Pager;

    // The query to filter assets.
    assetsQuery?: Query;

    // The folder path.
    path: ReadonlyArray<AssetPathItem>;

    // The parent folder.
    parentId: string;

    // Indicates if the assets are loaded once.
    isLoadedOnce?: boolean;

    // Indicates if the assets are loaded.
    isLoaded?: boolean;

    // Indicates if the assets are loading.
    isLoading?: boolean;

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

    public assetFolders =
        this.project(x => x.assetFolders);

    public assetsQuery =
        this.project(x => x.assetsQuery);

    public assetsPager =
        this.project(x => x.assetsPager);

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
            assetFolders: [],
            assets: [],
            assetsPager: new Pager(0, 0, 30),
            parentId: ROOT_ITEM.id,
            path: [ROOT_ITEM],
            tagsAvailable: {},
            tagsSelected: {}
        });
    }

    public loadAndListen(synchronizer: StateSynchronizer) {
        synchronizer.mapTo(this)
            .withPager('assetsPager', 'assets', 20)
            .withString('parentId', 'parent')
            .withStrings('tagsSelected', 'tags')
            .withSynchronizer('assetsQuery', QueryFullTextSynchronizer.INSTANCE)
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

        const query: any = {
            take: this.snapshot.assetsPager.pageSize,
            skip: this.snapshot.assetsPager.skip
        };

        const withQuery = hasQuery(this.snapshot);

        if (withQuery) {
            if (this.snapshot.assetsQuery) {
                query.query = this.snapshot.assetsQuery;
            }

            const searchTags = Object.keys(this.snapshot.tagsSelected);

            if (searchTags.length > 0) {
                query.tags = searchTags;
            }
        } else {
            query.parentId = this.snapshot.parentId;
        }

        const assets$ =
            this.assetsService.getAssets(this.appName, query);

        const assetFolders$ =
            !withQuery ?
                this.assetsService.getAssetFolders(this.appName, this.snapshot.parentId) :
                of(EMPTY_FOLDERS);

        const tags$ =
            !withQuery || !this.snapshot.isLoadedOnce ?
                this.assetsService.getTags(this.appName) :
                of(this.snapshot.tagsAvailable);

        return forkJoin(([assets$, assetFolders$, tags$])).pipe(
            tap(([assets, assetFolders, tagsAvailable]) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:assets.reloaded');
                }

                const path = assetFolders.path ?
                    [ROOT_ITEM, ...assetFolders.path] :
                    [];

                this.next(s => ({
                    ...s,
                    assetFolders: assetFolders.items,
                    assets: assets.items,
                    assetsPager: s.assetsPager.setCount(assets.total),
                    canCreate: assets.canCreate,
                    canCreateFolders: assetFolders.canCreate,
                    isLoaded: true,
                    isLoadedOnce: true,
                    isLoading: false,
                    path,
                    tagsAvailable
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
            const assets = [asset, ...s.assets];
            const assetsPager = s.assetsPager.incrementCount();

            const tags = updateTags(s, asset);

            return { ...s, assets, assetsPager, ...tags };
        });
    }

    public createAssetFolder(folderName: string) {
        return this.assetsService.postAssetFolder(this.appName, { folderName, parentId: this.snapshot.parentId }).pipe(
            tap(assetFolder => {
                if (assetFolder.parentId !== this.snapshot.parentId) {
                    return;
                }

                this.next(s => {
                    const assetFolders = [...s.assetFolders, assetFolder].sortedByString(x => x.folderName);

                    return { ...s, assetFolders };
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
                    const assetFolders = s.assetFolders.replaceBy('id', updated);

                    return { ...s, assetFolders };
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
            const assetFolders = s.assetFolders.filter(x => x.id !== assetFolder.id);

            return { ...s, assetFolders };
        });

        return this.assetsService.putAssetItemParent(this.appName, assetFolder, { parentId }, assetFolder.version).pipe(
            catchError(error => {
                this.next(s => {
                    const assetFolders = [...s.assetFolders, assetFolder].sortedByString(x => x.folderName);

                    return { ...s, assetFolders };
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
                    const assetsPager = s.assetsPager.decrementCount();

                    const tags = updateTags(s, undefined, asset);

                    return { ...s, assets, assetsPager, ...tags };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public deleteAssetFolder(assetFolder: AssetFolderDto): Observable<any> {
        return this.assetsService.deleteAssetItem(this.appName, assetFolder, false, assetFolder.version).pipe(
            tap(() => {
                this.next(s => {
                    const assetFolders = s.assetFolders.filter(x => x.id !== assetFolder.id);

                    return { ...s, assetFolders };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public navigate(parentId: string) {
        this.next({ parentId, assetsQuery: undefined, tagsSelected: {} });

        return this.loadInternal(false);
    }

    public setPager(assetsPager: Pager) {
        this.next({ assetsPager });

        return this.loadInternal(false);
    }

    public searchInternal(query?: Query | null, tags?: TagsSelected) {
        this.next(s => {
            const newState = { ...s, assetsPager: s.assetsPager.reset() };

            if (query !== null) {
                newState.assetsQuery = query;
            }

            if (tags) {
                newState.tagsSelected = tags;
            }

            return newState;
        });

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

function hasQuery(state: Snapshot) {
    return (state.assetsQuery && !!state.assetsQuery.fullText) || Object.keys(state.tagsSelected).length > 0;
}

function getParent(path: ReadonlyArray<AssetPathItem>) {
    return path.length > 1 ? { folderName: 'i18n:assets.specialFolder.parent', id: path[path.length - 2].id } : undefined;
}

@Injectable()
export class AssetsDialogState extends AssetsState { }