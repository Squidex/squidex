/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { from, Observable, of, shareReplay } from 'rxjs';
import { UIOptions } from '@app/framework';
import { AssetDto, AssetsDto, AssetsService } from './../services/assets.service';
import { ContentDto, ContentsDto, ContentsService } from './../services/contents.service';
import { AppsState } from './apps.state';

abstract class ResolverBase<T extends { id: string }, TResult extends { items: ReadonlyArray<T> }> {
    private readonly items: { [id: string]: Deferred<T | undefined> } = {};
    private pending: { [id: string]: boolean } | null = null;

    public resolveMany(ids: ReadonlyArray<string>): Observable<TResult> {
        if (ids.length === 0) {
            return of(this.createResult([]));
        }

        const nonResolved: string[] = [];

        const promises: Promise<T | undefined>[] = [];

        for (const id of ids) {
            let deferred = this.items[id];

            if (!deferred) {
                deferred = new Deferred<T>();
                this.items[id] = deferred;

                nonResolved.push(id);
            }

            promises.push(deferred.promise);
        }

        if (nonResolved.length > 0) {
            if (this.pending === null) {
                this.pending = {};

                setTimeout(() => {
                    this.resolvePending();
                }, 100);
            }

            for (const id of nonResolved) {
                this.pending[id] = true;
            }
        }

        return from(this.buildPromise(promises));
    }

    private async buildPromise(promises: Promise<T | undefined>[]) {
        const promise = await Promise.all(promises);

        return this.createResult(promise.defined() as any);
    }

    private resolvePending() {
        if (!this.pending) {
            return;
        }

        const allIds = Object.keys(this.pending);

        if (allIds.length === 0) {
            return;
        }

        this.pending = null;

        for (const ids of chunkArray(allIds, 100)) {
            this.resolveIds(ids);
        }
    }

    protected abstract createResult(items: T[]): TResult;

    protected abstract loadMany(ids: string[]): Observable<TResult>;

    private resolveIds(ids: string[]) {
        this.loadMany(ids)
            .subscribe({
                next: results => {
                    for (const id of ids) {
                        const content = results.items.find(x => x.id === id);

                        this.items[id]?.resolve(content);
                    }
                },
                error: ex => {
                    for (const id of ids) {
                        this.items[id]?.reject(ex);
                    }
                },
            });
    }
}

@Injectable()
export class ResolveContents extends ResolverBase<ContentDto, ContentsDto> {
    private readonly schemas: { [name: string]: Observable<ContentsDto> } = {};
    private readonly itemCount;

    constructor(
        uiOptions: UIOptions,
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService,
    ) {
        super();

        this.itemCount = uiOptions.get('referencesDropdownItemCount');
    }

    public resolveAll(schema: string) {
        let result = this.schemas[schema];

        if (!result) {
            result = this.contentsService.getContents(this.appName, schema, { take: this.itemCount }).pipe(shareReplay(1));

            this.schemas[schema] = result;
        }

        return result;
    }

    protected createResult(items: ContentDto[]): ContentsDto {
        return { items, total: items.length } as any;
    }

    protected loadMany(ids: string[]) {
        return this.contentsService.getAllContents(this.appName, { ids });
    }

    private get appName() {
        return this.appsState.appName;
    }
}

@Injectable()
export class ResolveAssets extends ResolverBase<AssetDto, AssetsDto> {
    constructor(
        private readonly appsState: AppsState,
        private readonly assetsService: AssetsService,
    ) {
        super();
    }

    protected createResult(items: AssetDto[]): AssetsDto {
        return { items, total: items.length } as any;
    }

    protected loadMany(ids: string[]) {
        return this.assetsService.getAssets(this.appName, { ids });
    }

    private get appName() {
        return this.appsState.appName;
    }
}

function chunkArray<T>(array: T[], size: number): T[][] {
    if (array.length > size) {
        return [array.slice(0, size), ...chunkArray(array.slice(size), size)];
    } else {
        return [array];
    }
}

class Deferred<T> {
    private handleResolve?: Function;
    private handleReject?: Function;
    private isHandled = false;

    public readonly promise: Promise<T>;

    constructor() {
        this.promise = new Promise<T>((resolve, reject) => {
            this.handleResolve = resolve;
            this.handleReject = reject;
        });
    }

    public resolve(value: T | PromiseLike<T>) {
        if (this.isHandled) {
            return;
        }

        this.isHandled = true;
        this.handleResolve?.(value);
    }

    public reject(reason?: any) {
        if (this.isHandled) {
            return;
        }

        this.isHandled = true;
        this.handleReject?.(reason);
    }
}
