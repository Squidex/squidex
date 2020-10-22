/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { DialogService, ErrorDto, Pager, shareSubscribed, State, StateSynchronizer, Types, Version, Versioned } from '@app/framework';
import { EMPTY, forkJoin, Observable, of } from 'rxjs';
import { catchError, finalize, map, switchMap, tap } from 'rxjs/operators';
import { ContentDto, ContentsService, StatusInfo } from './../services/contents.service';
import { SchemaDto } from './../services/schemas.service';
import { AppsState } from './apps.state';
import { SavedQuery } from './queries';
import { Query, QuerySynchronizer } from './query';
import { SchemasState } from './schemas.state';

type Updated = { content: ContentDto, error?: ErrorDto };

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

    public get appName() {
        return this.appsState.appName;
    }

    public get appId() {
        return this.appsState.appId;
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService,
        private readonly dialogs: DialogService
    ) {
        super({
            contents: [],
            contentsPager: new Pager(0)
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
                        return this.contentsService.getContent(this.appName, this.schemaName, id).pipe(catchError(() => of(null)));
                    } else {
                        return of(content);
                    }
                }));
    }

    public loadAndListen(synchronizer: StateSynchronizer) {
        synchronizer.mapTo(this)
            .keep('selectedContent')
            .withPager('contentsPager', 'contents', 10)
            .withSynchronizer('contentsQuery', QuerySynchronizer.INSTANCE)
            .whenSynced(() => this.loadInternal(false))
            .build();
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState({ selectedContent: this.snapshot.selectedContent });
        }

        return this.loadInternal(isReload);
    }

    public loadIfNotLoaded(): Observable<any> {
        if (this.snapshot.isLoaded) {
            return EMPTY;
        }

        return this.loadInternal(false);
    }

    private loadInternal(isReload: boolean) {
        return this.loadInternalCore(isReload).pipe(shareSubscribed(this.dialogs));
    }

    private loadInternalCore(isReload: boolean) {
        if (!this.appName || !this.schemaName) {
            return EMPTY;
        }

        this.next({ isLoading: true });

        const query: any = {
             take: this.snapshot.contentsPager.pageSize,
             skip: this.snapshot.contentsPager.skip
        };

        if (this.snapshot.contentsQuery) {
            query.query = this.snapshot.contentsQuery;
        }

        return this.contentsService.getContents(this.appName, this.schemaName, query).pipe(
            tap(({ total, items: contents, canCreate, canCreateAndPublish, statuses }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:contents.reloaded');
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
        return this.contentsService.getVersionData(this.appName, this.schemaName, content.id, version).pipe(
            shareSubscribed(this.dialogs));
    }

    public create(request: any, publish: boolean): Observable<ContentDto> {
        return this.contentsService.postContent(this.appName, this.schemaName, request, publish).pipe(
            tap(payload => {
                this.dialogs.notifyInfo('i18n:contents.created');

                return this.next(s => {
                    const contents = [payload, ...s.contents];
                    const contentsPager = s.contentsPager.incrementCount();

                    return { ...s, contents, contentsPager };
                });
            }),
            shareSubscribed(this.dialogs, {silent: true}));
    }

    public changeManyStatus(contentsToChange: ReadonlyArray<ContentDto>, status: string, dueTime: string | null): Observable<any> {
        return this.updateManyStatus(contentsToChange, status, dueTime).pipe(
            tap(results => {
                const errors = results.filter(x => !!x.error);

                if (errors.length > 0) {
                    const errror = errors[0].error!;

                    if (errors.length === contentsToChange.length) {
                        throw errror;
                    } else {
                        this.dialogs.notifyError(errror);
                    }
                }

                this.next(s => {
                    let contents = s.contents;

                    for (const updated of results.filter(x => !x.error).map(x => x.content)) {
                        contents = contents.replaceBy('id', updated);
                    }

                    return { ...s, contents };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public deleteMany(contentsToDelete: ReadonlyArray<ContentDto>) {
        return this.deleteManyCore(contentsToDelete, true).pipe(
            switchMap(results => {
                const referenced = results.filter(x => x.error?.statusCode === 400).map(x => x.content);

                if (referenced.length > 0) {
                    return this.dialogs.confirm(
                        'i18n:contents.deleteReferrerConfirmTitle',
                        'i18n:contents.deleteReferrerConfirmText',
                        'deleteReferencedAsset'
                    ).pipe(
                        switchMap(confirmed => {
                            if (confirmed) {
                                return this.deleteManyCore(referenced, false);
                            } else {
                                return of([]);
                            }
                        })
                    );
                } else {
                    return of(results);
                }
            }),
            tap(results => {
                const errors = results.filter(x => !!x.error);

                if (errors.length > 0) {
                    const errror = errors[0].error!;

                    if (errors.length === contentsToDelete.length) {
                        throw errror;
                    } else {
                        this.dialogs.notifyError(errror);
                    }
                }

                this.next(s => {
                    let contents = s.contents;
                    let contentsPager = s.contentsPager;

                    for (const content of results.filter(x => !x.error).map(x => x.content)) {
                        contents = contents.filter(x => x.id !== content.id);
                        contentsPager = contentsPager.decrementCount();
                    }

                    return { ...s, contents, contentsPager };
                });
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public changeStatus(content: ContentDto, status: string, dueTime: string | null): Observable<ContentDto> {
        return this.contentsService.putStatus(this.appName, content, status, dueTime, content.version).pipe(
            tap(updated => {
                this.replaceContent(updated, content.version, 'i18n:contents.updated');
            }),
            shareSubscribed(this.dialogs));
    }

    public update(content: ContentDto, request: any): Observable<ContentDto> {
        return this.contentsService.putContent(this.appName, content, request, content.version).pipe(
            tap(updated => {
                this.replaceContent(updated, content.version, 'i18n:contents.updated');
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public createDraft(content: ContentDto): Observable<ContentDto> {
        return this.contentsService.createVersion(this.appName, content, content.version).pipe(
            tap(updated => {
                this.replaceContent(updated, content.version, 'i18n:contents.updated');
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public deleteDraft(content: ContentDto): Observable<ContentDto> {
        return this.contentsService.deleteVersion(this.appName, content, content.version).pipe(
            tap(updated => {
                this.replaceContent(updated, content.version, 'i18n:contents.updated');
            }),
            shareSubscribed(this.dialogs));
    }

    public patch(content: ContentDto, request: any): Observable<ContentDto> {
        return this.contentsService.patchContent(this.appName, content, request, content.version).pipe(
            tap(updated => {
                this.replaceContent(updated, content.version, 'i18n:contents.updated');
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

    private deleteManyCore(contents: ReadonlyArray<ContentDto>, checkReferrers: boolean): Observable<ReadonlyArray<Updated>> {
        return forkJoin(
            contents.map(c => this.deleteCore(c, checkReferrers)));
    }

    private updateManyStatus(contents: ReadonlyArray<ContentDto>, status: string, dueTime: string | null): Observable<ReadonlyArray<Updated>> {
        return forkJoin(
            contents.map(c => this.updateStatus(c, status, dueTime)));
    }

    private deleteCore(content: ContentDto, checkReferrers: boolean): Observable<Updated> {
        return this.contentsService.deleteContent(this.appName, content, checkReferrers, content.version).pipe(
            map(() => ({ content })), catchError(error => of({ content, error })));
    }

    private updateStatus(content: ContentDto, status: string, dueTime: string | null): Observable<Updated> {
        return this.contentsService.putStatus(this.appName, content, status, dueTime, content.version).pipe(
            map(x => ({ content: x })), catchError(error => of({ content, error })));
    }

    public abstract get schemaId(): string;

    public abstract get schemaName(): string;
}

@Injectable()
export class ContentsState extends ContentsStateBase {
    constructor(appsState: AppsState, contentsService: ContentsService, dialogs: DialogService,
        private readonly schemasState: SchemasState
    ) {
        super(appsState, contentsService, dialogs);
    }

    public get schemaId() {
        return this.schemasState.schemaId;
    }

    public get schemaName() {
        return this.schemasState.schemaName;
    }
}

@Injectable()
export class ManualContentsState extends ContentsStateBase {
    public schema: SchemaDto;

    constructor(
        appsState: AppsState, contentsService: ContentsService, dialogs: DialogService
    ) {
        super(appsState, contentsService, dialogs);
    }

    public get schemaId() {
        return this.schema.id;
    }

    public get schemaName() {
        return this.schema.name;
    }
}

function buildStatusQueries(statuses: ReadonlyArray<StatusInfo> | undefined): ReadonlyArray<SavedQuery> {
    return statuses?.map(s => buildStatusQuery(s)) || [];
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
