/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { combineLatest, Observable } from 'rxjs';
import { distinctUntilChanged, map, tap } from 'rxjs/operators';

import {
    DialogService,
    ImmutableArray,
    notify,
    Pager,
    State
} from '@app/framework';

import { AssetDto, AssetsService} from './../services/assets.service';
import { AppsState } from './apps.state';

interface Snapshot {
    tags: { [name: string]: number };
    tagsSelected: { [name: string]: boolean };

    assets: ImmutableArray<AssetDto>;
    assetsPager: Pager;
    assetsQuery?: string;

    isLoaded?: false;
}

@Injectable()
export class AssetsState extends State<Snapshot> {
    public tags =
        this.changes.pipe(map(x => x.tags),
            distinctUntilChanged(), map(x => sort(x)));

    public tagsNames =
        this.tags.pipe(
            distinctUntilChanged(), map(x => x.map(t => t.name)));

    public assets =
        this.changes.pipe(map(x => x.assets),
            distinctUntilChanged());

    public assetsPager =
        this.changes.pipe(map(x => x.assetsPager),
            distinctUntilChanged());

    public isLoaded =
        this.changes.pipe(map(x => !!x.isLoaded),
            distinctUntilChanged());

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
            tap(dtos => {
                if (isReload) {
                    this.dialogs.notifyInfo('Assets reloaded.');
                }

                this.next(s => {
                    const assets = ImmutableArray.of(dtos[0].items);
                    const assetsPager = s.assetsPager.setCount(dtos[0].total);

                    return { ...s, assets, assetsPager, isLoaded: true, tags: dtos[1] };
                });
            }),
            notify(this.dialogs));
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
        return this.assetsService.deleteAsset(this.appName, asset.id, asset.version).pipe(
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
            notify(this.dialogs));
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

    public resetTags(): Observable<any> {
        this.next(s => ({ ...s, assetsPager: new Pager(0, 0, 30), tagsSelected: {} }));

        return this.loadInternal();
    }

    public search(query: string): Observable<any> {
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
    return Object.keys(tags).sort((a, b) => {
        if (a < b) {
            return -1;
        }
        if (a > b) {
            return 1;
        }
        return 0;
    }).map(key => {
        return { name: key, count: tags[key] };
    });
}

@Injectable()
export class AssetsDialogState extends AssetsState { }