/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { EMPTY, forkJoin, Observable, of, throwError } from 'rxjs';
import { catchError, finalize, switchMap, tap } from 'rxjs/operators';
import { compareStrings, DialogService, ErrorDto, getPagingInfo, ListState, MathHelper, shareSubscribed, State, Types } from '@app/framework';
import { AnnotateAssetDto, AssetDto, AssetFolderDto, AssetFoldersDto, AssetsService, RenameAssetFolderDto } from './../services/assets.service';
import { Query } from './../services/query';
import { AppsState } from './apps.state';

export type AssetPathItem = { id: string; folderName: string };

export type TagsAvailable = { [name: string]: number };
export type TagsSelected = { [name: string]: boolean };
export type TagItem = { name: string; count: number };

export const ROOT_ITEM: AssetPathItem = { id: MathHelper.EMPTY_GUID, folderName: 'i18n:assets.specialFolder.root' };

const EMPTY_FOLDERS: AssetFoldersDto = { canCreate: false, items: [] } as any;

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

    // The ref asset.
    ref?: string | null;

    // The parent folder.
    parentId: string;

    // Indicates if the assets are loaded once.
    isLoadedOnce?: boolean;

    // Indicates if the user can create assets.
    canCreate?: boolean;

    // Indicates if the user can create asset folders.
    canCreateFolders?: boolean;

    // Indicates if the user can rename asset tags.
    canRenameTag?: boolean;
}

export abstract class AssetsStateBase extends State<Snapshot> {
    public tagsUnsorted =
        this.project(x => x.tagsAvailable);

    public tagsSelected =
        this.project(x => x.tagsSelected);

    public tags =
        this.projectFrom(this.tagsUnsorted, getSortedTags);

    public tagsNames =
        this.projectFrom(this.tagsUnsorted, getTagNames);

    public selectedTagNames =
        this.projectFrom(this.tagsSelected, getTagNames);

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

    public canRenameTag =
        this.project(x => x.canRenameTag === true);

    public get parentId() {
        return this.snapshot.parentId;
    }

    public get appId() {
        return this.appsState.appId;
    }

    public get appName() {
        return this.appsState.appName;
    }

    protected constructor(name: string,
        private readonly appsState: AppsState,
        private readonly assetsService: AssetsService,
        private readonly dialogs: DialogService,
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
            total: 0,
        }, name);
    }

    public load(isReload = false, noSlowTotal = true, update: Partial<Snapshot> = {}): Observable<any> {
        if (!isReload) {
            this.resetState(update, 'Loading Initial');
        }

        return this.loadInternal(isReload, noSlowTotal);
    }

    private loadInternal(isReload: boolean, noSlowTotal: boolean): Observable<any> {
        this.next({ isLoading: true }, 'Loading Started');

        const query = createQuery(this.snapshot, noSlowTotal);

        const assets$ =
            this.assetsService.getAssets(this.appName, query);

        const assetFolders$ =
            query.parentId ?
                this.assetsService.getAssetFolders(this.appName, this.snapshot.parentId, 'PathAndItems') :
                of(EMPTY_FOLDERS);

        const tags$ =
            query.parentId || !this.snapshot.isLoadedOnce ?
                this.assetsService.getTags(this.appName) :
                of(this.snapshot.tagsAvailable);

        return forkJoin(([assets$, assetFolders$, tags$])).pipe(
            tap(([assetsResult, foldersResult, tagsAvailable]) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:assets.reloaded');
                }

                const path = foldersResult.path ?
                    [ROOT_ITEM, ...foldersResult.path] :
                    [];

                const { items: assets, total } = assetsResult;

                this.next(s => ({
                    ...s,
                    assets,
                    folders: foldersResult.items,
                    canCreate: assetsResult.canCreate,
                    canCreateFolders: foldersResult.canCreate,
                    canRenameTag: assetsResult.canRenameTag,
                    isLoaded: true,
                    isLoadedOnce: true,
                    isLoading: false,
                    path,
                    tagsAvailable,
                    total: total >= 0 ? total : s.total,
                }), 'Loading Success');
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
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
        }, 'Asset Created');
    }

    public createAssetFolder(folderName: string) {
        return this.assetsService.postAssetFolder(this.appName, { folderName, parentId: this.snapshot.parentId }).pipe(
            tap(folder => {
                if (folder.parentId !== this.snapshot.parentId) {
                    return;
                }

                this.next(s => {
                    const folders = [...s.folders, folder].sortByString(x => x.folderName);

                    return { ...s, folders };
                }, 'Folder Created');
            }),
            shareSubscribed(this.dialogs));
    }

    public updateAsset(asset: AssetDto, request: AnnotateAssetDto) {
        return this.assetsService.putAsset(this.appName, asset, request, asset.version).pipe(
            tap(updated => {
                this.next(s => {
                    const tags = updateTags(s, updated);

                    const assets = s.assets.replacedBy('id', updated);

                    return { ...s, assets, ...tags };
                }, 'Asset Updated');
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public updateAssetFolder(folder: AssetFolderDto, request: RenameAssetFolderDto) {
        return this.assetsService.putAssetFolder(this.appName, folder, request, folder.version).pipe(
            tap(updated => {
                this.next(s => {
                    const folders = s.folders.replacedBy('id', updated);

                    return { ...s, folders };
                }, 'Folder Updated');
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
        }, 'Asset Moving Started');

        return this.assetsService.putAssetItemParent(this.appName, asset, { parentId }, asset.version).pipe(
            catchError(error => {
                this.next(s => {
                    const assets = [asset, ...s.assets];

                    return { ...s, assets };
                }, 'Asset Moving Failed');

                return throwError(() => error);
            }),
            shareSubscribed(this.dialogs));
    }

    public moveAssetFolder(folder: AssetFolderDto, parentId?: string) {
        if (folder.id === parentId || folder.parentId === parentId) {
            return EMPTY;
        }

        this.next(s => {
            const folders = s.folders.filter(x => x.id !== folder.id);

            return { ...s, folders };
        }, 'Folder Moving Started');

        return this.assetsService.putAssetItemParent(this.appName, folder, { parentId }, folder.version).pipe(
            catchError(error => {
                this.next(s => {
                    const folders = [...s.folders, folder].sortByString(x => x.folderName);

                    return { ...s, folders };
                }, 'Folder Moving Done');

                return throwError(() => error);
            }),
            shareSubscribed(this.dialogs));
    }

    public deleteAsset(asset: AssetDto) {
        return this.assetsService.deleteAssetItem(this.appName, asset, true, asset.version).pipe(
            catchError((error: ErrorDto) => {
                if (isReferrerError(error)) {
                    return this.dialogs.confirm(
                        'i18n:assets.deleteReferrerConfirmTitle',
                        'i18n:assets.deleteReferrerConfirmText',
                        'deleteReferencedAsset',
                    ).pipe(
                        switchMap(confirmed => {
                            if (confirmed) {
                                return this.assetsService.deleteAssetItem(this.appName, asset, false, asset.version);
                            } else {
                                return EMPTY;
                            }
                        }),
                    );
                } else {
                    return throwError(() => error);
                }
            }),
            tap(() => {
                this.next(s => {
                    const assets = s.assets.filter(x => x.id !== asset.id);

                    const tags = updateTags(s, undefined, asset);

                    return { ...s, assets, total: s.total - 1, ...tags };
                }, 'Asset Deleted');
            }),
            shareSubscribed(this.dialogs));
    }

    public deleteAssetFolder(folder: AssetFolderDto): Observable<any> {
        return this.assetsService.deleteAssetItem(this.appName, folder, false, folder.version).pipe(
            tap(() => {
                this.next(s => {
                    const folders = s.folders.filter(x => x.id !== folder.id);

                    return { ...s, folders };
                }, 'Folder Deleted');
            }),
            shareSubscribed(this.dialogs));
    }

    public renameTag(name: string, tagName: string): Observable<any> {
        return this.assetsService.putTag(this.appName, name, { tagName }).pipe(
            tap(tags => {
                this.next(s => {
                    const tagsAvailable = tags;

                    return { ...s, tagsAvailable };
                }, 'Tag Renamed');
            }),
            shareSubscribed(this.dialogs));
    }

    public navigate(parentId: string) {
        if (!this.next({ parentId, query: undefined, tagsSelected: {} }, 'Loading Navigated')) {
            return EMPTY;
        }

        return this.loadInternal(false, true);
    }

    public page(paging: { page: number; pageSize: number }) {
        if (!this.next(paging, 'Loading Paged')) {
            return EMPTY;
        }

        return this.loadInternal(false, true);
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
        const update: Partial<Snapshot> = { page: 0, ref: null, total: 0 };

        if (query !== null) {
            update.query = query;
        }

        if (tags) {
            update.tagsSelected = tags;
        }

        if (!this.next(update, 'Loading Searched')) {
            return EMPTY;
        }

        return this.loadInternal(false, true);
    }
}

function getTagNames(tags: object): ReadonlyArray<string> {
    return Object.keys(tags);
}

function isReferrerError(error?: ErrorDto) {
    return error?.errorCode === 'OBJECT_REFERENCED';
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

function createQuery(snapshot: Snapshot, noSlowTotal: boolean) {
    const {
        ref,
        page,
        pageSize,
        query,
        tagsSelected,
        total,
    } = snapshot;

    const result: any = { take: pageSize, skip: pageSize * page, noSlowTotal };

    const tags = Object.keys(tagsSelected);

    if (Types.isString(ref)) {
        result.ref = ref;
    } else if (Types.isString(query?.fullText) || tags.length > 0) {
        if (query) {
            result.query = query;
        }

        if (tags.length > 0) {
            result.tags = tags;
        }
    } else {
        result.parentId = snapshot.parentId;
    }

    if (page > 0 && total > 0) {
        result.noTotal = true;
    }

    return result;
}

function getParent(path: ReadonlyArray<AssetPathItem>) {
    if (path.length > 1) {
        return { folderName: 'i18n:assets.specialFolder.parent', id: path[path.length - 2].id };
    } else {
        return undefined;
    }
}

function getSortedTags(tags: { [name: string]: number }) {
    return Object.keys(tags).sort(compareStrings).map(name => ({ name, count: tags[name] }));
}

@Injectable()
export class AssetsState extends AssetsStateBase {
    constructor(
        appsState: AppsState, assetsService: AssetsService, dialogs: DialogService,
    ) {
        super('Assets', appsState, assetsService, dialogs);
    }
}

@Injectable()
export class ComponentAssetsState extends AssetsStateBase {
    constructor(
        appsState: AppsState, assetsService: AssetsService, dialogs: DialogService,
    ) {
        super('Component Assets', appsState, assetsService, dialogs);
    }
}
