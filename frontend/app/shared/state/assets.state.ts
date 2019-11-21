/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { forkJoin, Observable, throwError } from 'rxjs';
import { catchError, finalize, tap } from 'rxjs/operators';

import {
    compareStrings,
    DialogService,
    LocalStoreService,
    Pager,
    shareSubscribed,
    State
} from '@app/framework';

import {
    AnnotateAssetDto,
    AssetDto,
    AssetFolderDto,
    AssetsService,
    RenameAssetFolderDto
} from './../services/assets.service';

import { AppsState } from './apps.state';
import { SavedQuery } from './queries';
import { encodeQuery, Query } from './query';

export type AssetPathItem = { id: string, folderName: string };

type TagsAvailable = { [name: string]: number };
type TagsSelected = { [name: string]: boolean };

const ROOT_PATH: ReadonlyArray<AssetPathItem> = [{ id: '00000000-0000-0000-0000-000000000000', folderName: 'Assets' }];

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

    // The json of the assets query.
    assetsQueryJson: string;

    // The folder path.
    path: ReadonlyArray<AssetPathItem>;

    // Indicates if the assets are loaded.
    isLoaded?: boolean;

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

    public path =
        this.project(x => x.path);

    public pathParent =
        this.project(x => getParent(x.path));

    public pathAvailable =
        this.project(x => x.path.length > 0);

    public pathRoot =
        this.project(x => x.path[x.path.length - 1]);

    public canCreate =
        this.project(x => x.canCreate === true);

    public canCreateFolders =
        this.project(x => x.canCreateFolders === true);

    constructor(
        private readonly appsState: AppsState,
        private readonly assetsService: AssetsService,
        private readonly dialogs: DialogService,
        private readonly localStore: LocalStoreService
    ) {
        super({
            assetFolders: [],
            assets: [],
            assetsPager: Pager.fromLocalStore('assets', localStore, 30),
            assetsQueryJson: '',
            path: ROOT_PATH,
            tagsAvailable: {},
            tagsSelected: {}
        });

        this.assetsPager.subscribe(pager => {
            pager.saveTo('assets', this.localStore);
        });
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload = false): Observable<any> {
        if (isReload) {
            this.next({ isLoaded: false });
        } else {
            this.next({
                assetFolders: [],
                assets: [],
                isLoaded: false
            });
        }

        const path = this.snapshot.path;

        const searchTags = Object.keys(this.snapshot.tagsSelected);

        const observables: Observable<any>[] = [
            this.assetsService.getAssets(this.appName,
                this.snapshot.assetsPager.pageSize,
                this.snapshot.assetsPager.skip,
                this.snapshot.assetsQuery,
                searchTags, undefined, this.parentId).pipe(
                tap(({ items: assets, total, canCreate }) => {
                    this.next(s => {
                        const assetsPager = s.assetsPager.setCount(total);

                        return { ...s, assets, assetsPager, canCreate };
                    });
                }))
        ];

        if (path.length === 1) {
            observables.push(
                this.assetsService.getTags(this.appName).pipe(
                    tap(tagsAvailable => {
                        this.next({ tagsAvailable });
                    })));
        }

        if (path.length > 0) {
            observables.push(
                this.assetsService.getAssetFolders(this.appName, this.parentId).pipe(
                    tap(({ items: assetFolders, canCreate: canCreateFolders }) => {
                        this.next({ assetFolders, canCreateFolders });
                    })));
        }

        return forkJoin(observables).pipe(
            tap(() => {
                if (isReload) {
                    this.dialogs.notifyInfo('Assets reloaded.');
                }
            }),
            finalize(() => {
                this.next({ isLoaded: true });
            }),
            shareSubscribed(this.dialogs));
    }

    public addAsset(asset: AssetDto) {
        if (asset.parentId !== this.parentId) {
            return;
        }

        this.next(s => {
            const assets = [asset, ...s.assets];
            const assetsPager = s.assetsPager.incrementCount();

            const tags = updateTags(s, asset);

            return { ...s, assets, assetsPager, ...tags };
        });
    }

    public createFolderFolder(folderName: string) {
        return this.assetsService.postAssetFolder(this.appName, { folderName, parentId: this.parentId }).pipe(
            tap(assetFolder => {
                if (assetFolder.parentId !== this.parentId) {
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
        if (asset.id === parentId) {
            return;
        }

        this.next(s => {
            const assets = s.assets.filter(x => x.id !== asset.id);
            const assetsPager = s.assetsPager.decrementCount();

            return { ...s, assets, assetsPager };
        });

        return this.assetsService.putAssetItemParent(this.appName, asset, { parentId }, asset.version).pipe(
            catchError(error => {
                this.next(s => {
                    const assets = [asset, ...s.assets];
                    const assetsPager = s.assetsPager.incrementCount();

                    return { ...s, assets, assetsPager };
                });

                return throwError(error);
            }),
            shareSubscribed(this.dialogs));
    }

    public moveAssetFolder(assetFolder: AssetFolderDto, parentId?: string) {
        if (assetFolder.id === parentId) {
            return;
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

    public deleteAsset(asset: AssetDto): Observable<any> {
        return this.assetsService.deleteAssetItem(this.appName, asset, asset.version).pipe(
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
        return this.assetsService.deleteAssetItem(this.appName, assetFolder, assetFolder.version).pipe(
            tap(() => {
                this.next(s => {
                    const assetFolders = s.assetFolders.filter(x => x.id !== assetFolder.id);

                    return { ...s, assetFolders };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public setPager(assetsPager: Pager) {
        this.next({ assetsPager });

        return this.loadInternal();
    }

    public searchInternal(query?: Query | null, tags?: TagsSelected) {
        this.next(s => {
            const newState = { ...s, assetsPager: s.assetsPager.reset() };

            if (query !== null) {
                newState.assetsQuery = query;
                newState.assetsQueryJson = encodeQuery(query);
            }

            if (tags) {
                newState.tagsSelected = tags;
            }

            if (Object.keys(newState.tagsSelected).length > 0 || (newState.assetsQuery && newState.assetsQuery.fullText)) {
                newState.path = [];
                newState.assetFolders = [];
            } else if (newState.path.length === 0) {
                newState.path = ROOT_PATH;
            }

            return newState;
        });

        return this.loadInternal();
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

    public navigate(folder: AssetPathItem) {
        this.next(s => {
            let path = s.path;

            const index = path.findIndex(x => x.id === folder.id);

            if (index >= 0) {
                path = path.slice(0, index + 1);
            } else {
                path = [...path, folder];
            }

            return { ...s, path };
        });

        return this.loadInternal();
    }

    public resetTags(): Observable<any> {
        return this.searchInternal(null, {});
    }

    public search(query?: Query): Observable<any> {
        return this.searchInternal(query);
    }

    public isQueryUsed(saved: SavedQuery) {
        return this.snapshot.assetsQueryJson === saved.queryJson;
    }

    public isTagSelected(tag: string) {
        return this.snapshot.tagsSelected[tag];
    }

    public isTagSelectionEmpty() {
        return Object.keys(this.snapshot.tagsSelected).length === 0;
    }

    public get parentId() {
        return this.snapshot.path.length > 0 ? this.snapshot.path[this.snapshot.path.length - 1].id : undefined;
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
    return path.length > 1 ? { folderName: '<Parent>', id: path[path.length - 2].id } : undefined;
}

@Injectable()
export class AssetsDialogState extends AssetsState { }