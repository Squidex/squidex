/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { empty, forkJoin, Observable, of } from 'rxjs';
import { catchError, finalize, switchMap, tap } from 'rxjs/operators';

import {
    DialogService,
    ErrorDto,
    LocalStoreService,
    Pager,
    shareSubscribed,
    State,
    Types,
    Version,
    Versioned
} from '@app/framework';

import { ContentDto, ContentsService, StatusInfo } from './../services/contents.service';
import { SchemaDto } from './../services/schemas.service';
import { AppsState } from './apps.state';
import { SavedQuery } from './queries';
import { Query } from './query';
import { SchemasState } from './schemas.state';

interface Snapshot {
    // The current comments.
    contents: ReadonlyArray<ContentDto>;

    // The pagination information.
    contentsPager: Pager;

    // The query to filter and sort contents.
    contentsQuery?: Query;

    // Indicates if the contents are loaded.
    isLoaded?: boolean;

    // Indicates if the contents are loading.
    isLoading?: boolean;

    // The statuses.
    statuses?: ReadonlyArray<StatusInfo>;

    // The selected content.
    selectedContent?: ContentDto | null;

    // Indicates if the user can create a content.
    canCreate?: boolean;

    // Indicates if the user can create and publish a content.
    canCreateAndPublish?: boolean;
}

export abstract class ContentsStateBase extends State<Snapshot> {
    private previousId: string;

    public selectedContent: Observable<ContentDto | null | undefined> =
        this.project(x => x.selectedContent, Types.equals);

    public contents =
        this.project(x => x.contents);

    public contentsPager =
        this.project(x => x.contentsPager);

    public contentsQuery =
        this.project(x => x.contentsQuery);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public canCreate =
        this.project(x => x.canCreate === true);

    public canCreateAndPublish =
        this.project(x => x.canCreateAndPublish === true);

    public canCreateAny =
        this.project(x => x.canCreate === true || x.canCreateAndPublish === true);

    public statuses =
        this.project(x => x.statuses);

    public statusQueries =
        this.projectFrom(this.statuses, x => buildStatusQueries(x));

    constructor(
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService,
        private readonly dialogs: DialogService,
        private readonly localStore: LocalStoreService
    ) {
        super({
            contents: [],
            contentsPager: Pager.fromLocalStore('contents', localStore)
        });

        this.contentsPager.subscribe(pager => {
            pager.saveTo('contents', this.localStore);
        });
    }

    public select(id: string | null): Observable<ContentDto | null> {
        return this.loadContent(id).pipe(
            tap(content => {
                this.next(s => {
                    const contents = content ? s.contents.replaceBy('id', content) : s.contents;

                    return { ...s, selectedContent: content, contents };
                });
            }));
    }

    private loadContent(id: string | null) {
        return !id ?
            of(null) :
            of(this.snapshot.contents.find(x => x.id === id)).pipe(
                switchMap(content => {
                    if (!content) {
                        return this.contentsService.getContent(this.appName, this.schemaId, id).pipe(catchError(() => of(null)));
                    } else {
                        return of(content);
                    }
                }));
    }

    public load(isReload = false): Observable<any> {
        if (!isReload && this.schemaId !== this.previousId) {
            const contentsPager = this.snapshot.contentsPager.reset();

            this.resetState({ contentsPager, selectedContent: this.snapshot.selectedContent });
        }

        return this.loadInternal(isReload);
    }

    public loadIfNotLoaded(): Observable<any> {
        if (this.snapshot.isLoaded) {
            return empty();
        }

        return this.loadInternal(false);
    }

    private loadInternal(isReload: boolean) {
        return this.loadInternalCore(isReload).pipe(shareSubscribed(this.dialogs));
    }

    private loadInternalCore(isReload: boolean) {
        if (!this.appName || !this.schemaId) {
            return empty();
        }

        this.next({ isLoading: true });

        this.previousId = this.schemaId;

        return this.contentsService.getContents(this.appName, this.schemaId,
                this.snapshot.contentsPager.pageSize,
                this.snapshot.contentsPager.skip,
                this.snapshot.contentsQuery, undefined).pipe(
            tap(({ total, items: contents, canCreate, canCreateAndPublish, statuses }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('Contents reloaded.');
                }

                return this.next(s => {
                    const contentsPager = s.contentsPager.setCount(total);

                    statuses = s.statuses || statuses;

                    let selectedContent = s.selectedContent;

                    if (selectedContent) {
                        selectedContent = contents.find(x => x.id === selectedContent!.id) || selectedContent;
                    }

                    return { ...s,
                        canCreate,
                        canCreateAndPublish,
                        contents,
                        contentsPager,
                        isLoaded: true,
                        isLoading: false,
                        selectedContent,
                        statuses
                    };
                });
            }),
            finalize(() => {
                this.next({ isLoading: false });
            }));
    }

    public loadVersion(content: ContentDto, version: Version): Observable<Versioned<any>> {
        return this.contentsService.getVersionData(this.appName, this.schemaId, content.id, version).pipe(
            shareSubscribed(this.dialogs));
    }

    public create(request: any, publish: boolean): Observable<ContentDto> {
        return this.contentsService.postContent(this.appName, this.schemaId, request, publish).pipe(
            tap(payload => {
                this.dialogs.notifyInfo('Content created successfully.');

                return this.next(s => {
                    const contents = [payload, ...s.contents];
                    const contentsPager = s.contentsPager.incrementCount();

                    return { ...s, contents, contentsPager };
                });
            }),
            shareSubscribed(this.dialogs, {silent: true}));
    }

    public changeManyStatus(contents: ReadonlyArray<ContentDto>, status: string, dueTime: string | null): Observable<any> {
        return forkJoin(
            contents.map(c =>
                this.contentsService.putStatus(this.appName, c, status, dueTime, c.version).pipe(
                    catchError(error => of(error))))).pipe(
            tap(results => {
                const error = results.find(x => x instanceof ErrorDto);

                if (error) {
                    this.dialogs.notifyError(error);
                }

                return of(error);
            }),
            switchMap(() => this.loadInternalCore(false)),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public deleteMany(contents: ReadonlyArray<ContentDto>): Observable<any> {
        return forkJoin(
            contents.map(c =>
                this.contentsService.deleteContent(this.appName, c, c.version).pipe(
                    catchError(error => of(error))))).pipe(
            tap(results => {
                const error = results.find(x => x instanceof ErrorDto);

                if (error) {
                    this.dialogs.notifyError(error);
                }

                return of(error);
            }),
            switchMap(() => this.loadInternal(false)),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public changeStatus(content: ContentDto, status: string, dueTime: string | null): Observable<ContentDto> {
        return this.contentsService.putStatus(this.appName, content, status, dueTime, content.version).pipe(
            tap(updated => {
                this.replaceContent(updated, content.version, 'Content updated successfully.');
            }),
            shareSubscribed(this.dialogs));
    }

    public update(content: ContentDto, request: any): Observable<ContentDto> {
        return this.contentsService.putContent(this.appName, content, request, content.version).pipe(
            tap(updated => {
                this.replaceContent(updated, content.version, 'Content updated successfully.');
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public createDraft(content: ContentDto): Observable<ContentDto> {
        return this.contentsService.createVersion(this.appName, content, content.version).pipe(
            tap(updated => {
                this.replaceContent(updated, content.version, 'Content updated successfully.');
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public deleteDraft(content: ContentDto): Observable<ContentDto> {
        return this.contentsService.deleteVersion(this.appName, content, content.version).pipe(
            tap(updated => {
                this.replaceContent(updated, content.version, 'Content updated successfully.');
            }),
            shareSubscribed(this.dialogs));
    }

    public patch(content: ContentDto, request: any): Observable<ContentDto> {
        return this.contentsService.patchContent(this.appName, content, request, content.version).pipe(
            tap(updated => {
                this.replaceContent(updated, content.version, 'Content updated successfully.');
            }),
            shareSubscribed(this.dialogs));
    }

    public search(contentsQuery?: Query): Observable<any> {
        this.next(s => ({ ...s, contentsPager: s.contentsPager.reset(), contentsQuery }));

        return this.loadInternal(false);
    }

    public setPager(contentsPager: Pager) {
        this.next(s => ({ ...s, contentsPager }));

        return this.loadInternal(false);
    }
    private get appName() {
        return this.appsState.appName;
    }

    private replaceContent(content: ContentDto, oldVersion?: Version, updateText?: string) {
        if (!oldVersion || !oldVersion.eq(content.version)) {
            if (updateText) {
                this.dialogs.notifyInfo(updateText);
            }

            return this.next(s => {
                const contents = s.contents.replaceBy('id', content);

                const selectedContent =
                    s.selectedContent &&
                    s.selectedContent.id === content.id ?
                    content :
                    s.selectedContent;

                return { ...s, contents, selectedContent };
            });
        }
    }

    protected abstract get schemaId(): string;
}

@Injectable()
export class ContentsState extends ContentsStateBase {
    constructor(appsState: AppsState, contentsService: ContentsService, dialogs: DialogService, localStore: LocalStoreService,
        private readonly schemasState: SchemasState
    ) {
        super(appsState, contentsService, dialogs, localStore);
    }

    protected get schemaId() {
        return this.schemasState.schemaName;
    }
}

@Injectable()
export class ManualContentsState extends ContentsStateBase {
    public schema: SchemaDto;

    constructor(
        appsState: AppsState, contentsService: ContentsService, dialogs: DialogService, localStore: LocalStoreService
    ) {
        super(appsState, contentsService, dialogs, localStore);
    }

    protected get schemaId() {
        return this.schema.name;
    }
}

function buildStatusQueries(statuses: ReadonlyArray<StatusInfo> | undefined): ReadonlyArray<SavedQuery> {
    return statuses ? statuses.map(s => buildStatusQuery(s)) : [];
}

function buildStatusQuery(s: StatusInfo) {
    const query = {
        filter: {
            and: [
                { path: 'status', op: 'eq', value: s.status }
            ]
        }
    };

    return ({ name: s.status, color: s.color, query });
}
