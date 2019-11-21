/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { forkJoin, Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';

import {
    compareStrings,
    DialogService,
    LocalStoreService,
    Pager,
    shareSubscribed,
    State
} from '@app/framework';

import {
    AssetDto,
    AssetFolderDto,
    AssetsService
} from './../services/assets.service';

import { AppsState } from './apps.state';
import { SavedQuery } from './queries';
import { encodeQuery, Query } from './query';

export type AssetPathItem = { id?: string, folderName: string };

type TagsAvailable = { [name: string]: number };
type TagsSelected = { [name: string]: boolean };

const ROOT_PATH: ReadonlyArray<AssetPathItem> = [{ folderName: 'Assets' }];

interface Snapshot {
    // All assets tags.
    tags: TagsAvailable;

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
        this.project(x => x.tags);

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

    public parent =
        this.project(x => getParent(x.path));

    public hasPath =
        this.project(x => x.path.length > 0);

    public root =
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
            path: ROOT_PATH,
            assets: [],
            assetFolders: [],
            assetsPager: Pager.fromLocalStore('assets', localStore, 30),
            assetsQueryJson: '',
            tags: {},
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
        const path = this.snapshot.path;
        const parentId = getParentId(path);

        const searchTags = Object.keys(this.snapshot.tagsSelected);

        const observables: Observable<any>[] = [
            this.assetsService.getAssets(this.appName,
                this.snapshot.assetsPager.pageSize,
                this.snapshot.assetsPager.skip,
                this.snapshot.assetsQuery,
                searchTags, undefined, parentId).pipe(
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
                    tap(tags => {
                        this.next(s => {
                            return { ...s, tags };
                        });
                    })));
        }

        if (path.length > 0) {
            observables.push(
                this.assetsService.getAssetFolders(this.appName, parentId).pipe(
                    tap(({ items: assetFolders, canCreate: canCreateFolders }) => {
                        this.next(s => {
                            return { ...s, assetFolders, canCreateFolders };
                        });
                    })));
        }

        return forkJoin(observables).pipe(
            finalize(() => {
                if (isReload) {
                    this.dialogs.notifyInfo('Assets reloaded.');
                }

                this.next(s => {
                    return { ...s, isLoaded: true };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public add(asset: AssetDto) {
        this.next(s => {
            const assets = [asset, ...s.assets];
            const assetsPager = s.assetsPager.incrementCount();

            const tags = { ...s.tags };

            addTags(asset, tags);

            return { ...s, assets, assetsPager, tags };
        });
    }

    public createFolder(folderName: string) {
        const parentId = getParentId(this.snapshot.path);

        return this.assetsService.postAssetFolder(this.appName, { folderName, parentId }).pipe(
            tap(folder => {
                this.next(s => {
                    const assetFolders = [...s.assetFolders, folder].sortedByString(x => x.folderName);

                    return { ...s, assetFolders };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public delete(asset: AssetDto): Observable<any> {
        return this.assetsService.deleteAsset(this.appName, asset, asset.version).pipe(
            tap(() => {
                this.next(s => {
                    const assets = s.assets.filter(x => x.id !== asset.id);
                    const assetsPager = s.assetsPager.decrementCount();

                    const tags = { ...s.tags };
                    const tagsSelected = { ...s.tagsSelected };

                    removeTags(asset, tags, tagsSelected);

                    return { ...s, assets, assetsPager, tags, tagsSelected };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public update(asset: AssetDto) {
        this.next(s => {
            const previous = s.assets.find(x => x.id === asset.id);

            const tags = { ...s.tags };
            const tagsSelected = { ...s.tagsSelected };

            if (previous) {
                removeTags(previous, tags, tagsSelected);
            }

            if (asset) {
                addTags(asset, tags);
            }

            const assets = s.assets.replaceBy('id', asset);

            return { ...s, assets, tags, tagsSelected };
        });
    }

    public setPager(assetsPager: Pager) {
        this.next(s => ({ ...s, assetsPager }));

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

            if (Object.keys(this.tagsSelected).length > 0 || (newState.assetsQuery && newState.assetsQuery.fullText)) {
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

            if (!folder.id) {
                path = ROOT_PATH;
            } else {
                const index = path.findIndex(x => x.id === folder.id);

                if (index >= 0) {
                    path = path.slice(0, index);
                } else {
                    path = [...path, folder];
                }
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

    private get appName() {
        return this.appsState.appName;
    }
}

function addTags(asset: AssetDto, tags: { [x: string]: number; }) {
    for (const tag of asset.tags) {
        if (tags[tag]) {
            tags[tag]++;
        } else {
            tags[tag] = 1;
        }
    }
}

function removeTags(previous: AssetDto, tags: { [x: string]: number; }, tagsSelected: { [x: string]: boolean; }) {
    for (const tag of previous.tags) {
        if (tags[tag] === 1) {
            delete tags[tag];
            delete tagsSelected[tag];
        } else {
            tags[tag]--;
        }
    }
}

function sort(tags: { [name: string]: number }) {
    return Object.keys(tags).sort(compareStrings).map(name => ({ name, count: tags[name] }));
}

function getParent(path: ReadonlyArray<AssetPathItem>) {
    return path.length > 1 ? { folderName: '...', id: path[path.length - 1].id } : undefined;
}

function getParentId(path: ReadonlyArray<AssetPathItem>) {
    return path.length > 1 ? path[path.length - 1].id : undefined;
}

@Injectable()
export class AssetsDialogState extends AssetsState { }