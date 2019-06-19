/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { combineLatest, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

import {
    compareStringsAsc,
    DialogService,
    ImmutableArray,
    Pager,
    shareSubscribed,
    State
} from '@app/framework';

import { AssetDto, AssetsService} from './../services/assets.service';
import { AppsState } from './apps.state';

interface Snapshot {
    // All assets tags.
    tags: { [name: string]: number };

    // The selected asset tags.
    tagsSelected: { [name: string]: boolean };

    // The current assets.
    assets: ImmutableArray<AssetDto>;

    // The pagination information.
    assetsPager: Pager;

    // The query to filter assets.
    assetsQuery?: string;

    // Indicates if the assets are loaded.
    isLoaded?: boolean;

    // Indicates if the user can create assets.
    canCreate?: boolean;
}

@Injectable()
export class AssetsState extends State<Snapshot> {
    public tags =
        this.project2(x => x.tags, x => sort(x));

    public tagsNames =
        this.project2(x => x.tags, x => Object.keys(x));

    public selectedTagNames =
        this.project2(x => x.tagsSelected, x => Object.keys(x));

    public assets =
        this.project(x => x.assets);

    public assetsQuery =
        this.project(x => x.assetsQuery);

    public assetsPager =
        this.project(x => x.assetsPager);

    public isLoaded =
        this.project(x => !!x.isLoaded);

    public canCreate =
        this.project(x => !!x.canCreate);

    constructor(
        private readonly appsState: AppsState,
        private readonly assetsService: AssetsService,
        private readonly dialogs: DialogService
    ) {
        super({ assets: ImmutableArray.empty(), assetsPager: new Pager(0, 0, 30), tags: {}, tagsSelected: {} });
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload = false): Observable<any> {
        return combineLatest(
            this.assetsService.getAssets(this.appName,
                this.snapshot.assetsPager.pageSize,
                this.snapshot.assetsPager.skip,
                this.snapshot.assetsQuery,
                Object.keys(this.snapshot.tagsSelected)),
            this.assetsService.getTags(this.appName)
        ).pipe(
            tap(([ { items, total, canCreate }, tags ]) => {
                if (isReload) {
                    this.dialogs.notifyInfo('Assets reloaded.');
                }

                this.next(s => {
                    const assets = ImmutableArray.of(items);
                    const assetsPager = s.assetsPager.setCount(total);

                    return { ...s, assets, assetsPager, isLoaded: true, tags, canCreate };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public add(asset: AssetDto) {
        this.next(s => {
            const assets = s.assets.pushFront(asset);
            const assetsPager = s.assetsPager.incrementCount();

            const tags = { ...s.tags };

            addTags(asset, tags);

            return { ...s, assets, assetsPager, tags };
        });
    }

    public delete(asset: AssetDto): Observable<any> {
        return this.assetsService.deleteAsset(this.appName, asset, asset.version).pipe(
            tap(() => {
                return this.next(s => {
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

    public toggleTag(tag: string): Observable<any> {
        this.next(s => {
            const tagsSelected = { ...s.tagsSelected };

            if (tagsSelected[tag]) {
                delete tagsSelected[tag];
            } else {
                tagsSelected[tag] = true;
            }

            return { ...s, assetsPager: new Pager(0, 0, 30), tagsSelected };
        });

        return this.loadInternal();
    }

    public selectTags(tags: string[]): Observable<any> {
        this.next(s => {
            const tagsSelected = {};

            for (let tag of tags) {
                tagsSelected[tag] = true;
            }

            return { ...s, assetsPager: new Pager(0, 0, 30), tagsSelected };
        });

        return this.loadInternal();
    }

    public resetTags(): Observable<any> {
        this.next(s => ({ ...s, assetsPager: new Pager(0, 0, 30), tagsSelected: {} }));

        return this.loadInternal();
    }

    public search(query?: string): Observable<any> {
        this.next(s => ({ ...s, assetsPager: new Pager(0, 0, 30), assetsQuery: query }));

        return this.loadInternal();
    }

    public goNext(): Observable<any> {
        this.next(s => ({ ...s, assetsPager: s.assetsPager.goNext() }));

        return this.loadInternal();
    }

    public goPrev(): Observable<any> {
        this.next(s => ({ ...s, assetsPager: s.assetsPager.goPrev() }));

        return this.loadInternal();
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
    for (let tag of asset.tags) {
        if (tags[tag]) {
            tags[tag]++;
        } else {
            tags[tag] = 1;
        }
    }
}

function removeTags(previous: AssetDto, tags: { [x: string]: number; }, tagsSelected: { [x: string]: boolean; }) {
    for (let tag of previous.tags) {
        if (tags[tag] === 1) {
            delete tags[tag];
            delete tagsSelected[tag];
        } else {
            tags[tag]--;
        }
    }
}

function sort(tags: { [name: string]: number }) {
    return Object.keys(tags).sort(compareStringsAsc).map(name => ({ name, count: tags[name] }));
}

@Injectable()
export class AssetsDialogState extends AssetsState { }