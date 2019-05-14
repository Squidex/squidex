/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { forkJoin, Observable, of } from 'rxjs';
import { catchError, distinctUntilChanged, map, switchMap, tap } from 'rxjs/operators';

import {
    DateTime,
    DialogService,
    ErrorDto,
    ImmutableArray,
    Pager,
    shareSubscribed,
    State,
    Version,
    Versioned
} from '@app/framework';

import { AuthService } from './../services/auth.service';
import { SchemaDto } from './../services/schemas.service';
import { AppsState } from './apps.state';
import { SchemasState } from './schemas.state';

import { ContentDto, ContentQueryStatus, ContentsService, ScheduleDto } from './../services/contents.service';

interface Snapshot {
    // The current comments.
    contents: ImmutableArray<ContentDto>;

    // The pagination information.
    contentsPager: Pager;

    // The query to filter and sort contents.
    contentsQuery?: string;

    // Indicates if the contents are loaded.
    isLoaded?: boolean;

    // Indicates which status is shown.
    status: ContentQueryStatus;

    // The selected content.
    selectedContent?: ContentDto | null;
}

export const CONTENT_STATUSES = {
    'PublishedDraft': 'Published and Drafts (Default)',
    'PublishedOnly': 'Published only',
    'Archived': 'Archived'
};

function sameContent(lhs: ContentDto, rhs?: ContentDto): boolean {
    return lhs === rhs || (!!lhs && !!rhs && lhs.id === rhs.id && lhs.version === rhs.version);
}

export abstract class ContentsStateBase extends State<Snapshot> {
    public selectedContent =
        this.changes.pipe(map(x => x.selectedContent),
            distinctUntilChanged(sameContent));

    public contents =
        this.changes.pipe(map(x => x.contents),
            distinctUntilChanged());

    public contentsPager =
        this.changes.pipe(map(x => x.contentsPager),
            distinctUntilChanged());

    public contentsQuery =
        this.changes.pipe(map(x => x.contentsQuery),
            distinctUntilChanged());

    public isLoaded =
        this.changes.pipe(map(x => !!x.isLoaded),
            distinctUntilChanged());

    public isArchive =
        this.changes.pipe(map(x => x.status === 'Archived'),
            distinctUntilChanged());

    public status =
        this.changes.pipe(map(x => x.status),
            distinctUntilChanged());

    constructor(
        private readonly appsState: AppsState,
        private readonly authState: AuthService,
        private readonly contentsService: ContentsService,
        private readonly dialogs: DialogService
    ) {
        super({ contents: ImmutableArray.of(), contentsPager: new Pager(0), status: 'PublishedDraft' });
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

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload = false) {
        return this.loadInternalCore(isReload).pipe(shareSubscribed(this.dialogs));
    }

    private loadInternalCore(isReload = false) {
        return this.contentsService.getContents(this.appName, this.schemaName,
                this.snapshot.contentsPager.pageSize,
                this.snapshot.contentsPager.skip,
                this.snapshot.contentsQuery, undefined,
                this.snapshot.status).pipe(
            tap(({ total, items }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('Contents reloaded.');
                }

                return this.next(s => {
                    const contents = ImmutableArray.of(items);
                    const contentsPager = s.contentsPager.setCount(total);

                    let selectedContent = s.selectedContent;

                    if (selectedContent) {
                        selectedContent = contents.find(x => x.id === selectedContent!.id) || selectedContent;
                    }

                    return { ...s, contents, contentsPager, selectedContent, isLoaded: true };
                });
            }));
    }

    public create(request: any, publish: boolean): Observable<ContentDto> {
        return this.contentsService.postContent(this.appName, this.schemaName, request, publish).pipe(
            tap(payload => {
                this.dialogs.notifyInfo('Contents created successfully.');

                return this.next(s => {
                    const contents = s.contents.pushFront(payload);
                    const contentsPager = s.contentsPager.incrementCount();

                    return { ...s, contents, contentsPager };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public changeManyStatus(contents: ContentDto[], action: string, dueTime: string | null): Observable<any> {
        return forkJoin(
            contents.map(c =>
                this.contentsService.changeContentStatus(this.appName, this.schemaName, c.id, action, dueTime, c.version).pipe(
                    catchError(error => of(error))))).pipe(
            tap(results => {
                const error = results.find(x => x instanceof ErrorDto);

                if (error) {
                    this.dialogs.notifyError(error);
                }

                return of(error);
            }),
            switchMap(() => this.loadInternalCore()),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public deleteMany(contents: ContentDto[]): Observable<any> {
        return forkJoin(
            contents.map(c =>
                this.contentsService.deleteContent(this.appName, this.schemaName, c.id, c.version).pipe(
                    catchError(error => of(error))))).pipe(
            tap(results => {
                const error = results.find(x => x instanceof ErrorDto);

                if (error) {
                    this.dialogs.notifyError(error);
                }

                return of(error);
            }),
            switchMap(() => this.loadInternal()),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public publishChanges(content: ContentDto, dueTime: string | null, now?: DateTime): Observable<ContentDto> {
        return this.contentsService.changeContentStatus(this.appName, this.schemaName, content.id, 'Publish', dueTime, content.version).pipe(
            map(({ version }) => {
                if (dueTime) {
                    return changeScheduleStatus(content, 'Published', dueTime, this.user, version, now);
                } else {
                    return confirmChanges(content, this.user, version, now);
                }
            }),
            tap(updated => {
                this.dialogs.notifyInfo('Content updated successfully.');

                this.replaceContent(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public changeStatus(content: ContentDto, action: string, status: string, dueTime: string | null, now?: DateTime): Observable<ContentDto> {
        return this.contentsService.changeContentStatus(this.appName, this.schemaName, content.id, action, dueTime, content.version).pipe(
            map(({ version }) => {
                if (dueTime) {
                    return changeScheduleStatus(content, status, dueTime, this.user, version, now);
                } else {
                    return changeStatus(content, status, this.user, version, now);
                }
            }),
            tap(updated => {
                this.dialogs.notifyInfo('Content updated successfully.');

                this.replaceContent(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public update(content: ContentDto, request: any, now?: DateTime): Observable<ContentDto> {
        return this.contentsService.putContent(this.appName, this.schemaName, content.id, request, false, content.version).pipe(
            map(({ payload, version }) => updateData(content, payload, this.user, version, now)),
            tap(updated => {
                this.dialogs.notifyInfo('Content updated successfully.');

                this.replaceContent(updated, content.version);
            }),
            shareSubscribed(this.dialogs));
    }

    public proposeUpdate(content: ContentDto, request: any, now?: DateTime): Observable<ContentDto> {
        return this.contentsService.putContent(this.appName, this.schemaName, content.id, request, true, content.version).pipe(
            map(({ payload, version }) => updateDataDraft(content, payload, this.user, version, now)),
            tap(updated => {
                this.dialogs.notifyInfo('Content updated successfully.');

                this.replaceContent(updated, content.version);
            }),
            shareSubscribed(this.dialogs));
    }

    public discardChanges(content: ContentDto, now?: DateTime): Observable<ContentDto> {
        return this.contentsService.discardChanges(this.appName, this.schemaName, content.id, content.version).pipe(
            map(({ version }) => discardChanges(content, this.user, version, now)),
            tap(updated => {
                this.dialogs.notifyInfo('Content updated successfully.');

                this.replaceContent(updated, content.version);
            }),
            shareSubscribed(this.dialogs));
    }

    public patch(content: ContentDto, request: any, now?: DateTime): Observable<ContentDto> {
        return this.contentsService.patchContent(this.appName, this.schemaName, content.id, request, content.version).pipe(
            map(({ payload, version }) => updateData(content, payload, this.user, version, now)),
            tap(updated => {
                this.dialogs.notifyInfo('Content updated successfully.');

                this.replaceContent(updated, content.version);
            }),
            shareSubscribed(this.dialogs));
    }

    private replaceContent(content: ContentDto, oldVersion?: Version) {
        if (!oldVersion || !oldVersion.eq(content.version)) {
            return this.next(s => {
                const contents = s.contents.replaceBy('id', content);
                const selectedContent = s.selectedContent && s.selectedContent.id === content.id ? content : s.selectedContent;

                return { ...s, contents, selectedContent };
            });
        }
    }

    public filterStatus(status: ContentQueryStatus): Observable<any> {
        this.next(s => ({ ...s, contentsPager: new Pager(0), contentsQuery: undefined, status }));

        return this.loadInternal();
    }

    public search(query?: string): Observable<any> {
        this.next(s => ({ ...s, contentsPager: new Pager(0), contentsQuery: query }));

        return this.loadInternal();
    }

    public goNext(): Observable<any> {
        this.next(s => ({ ...s, contentsPager: s.contentsPager.goNext() }));

        return this.loadInternal();
    }

    public goPrev(): Observable<any> {
        this.next(s => ({ ...s, contentsPager: s.contentsPager.goPrev() }));

        return this.loadInternal();
    }

    public loadVersion(content: ContentDto, version: Version): Observable<Versioned<any>> {
        return this.contentsService.getVersionData(this.appName, this.schemaName, content.id, version).pipe(
            shareSubscribed(this.dialogs));
    }

    private get appName() {
        return this.appsState.appName;
    }

    private get user() {
        return this.authState.user!.token;
    }

    protected abstract get schemaName(): string;
}

@Injectable()
export class ContentsState extends ContentsStateBase {
    constructor(appsState: AppsState, authState: AuthService, contentsService: ContentsService, dialogs: DialogService,
        private readonly schemasState: SchemasState
    ) {
        super(appsState, authState, contentsService, dialogs);
    }

    protected get schemaName() {
        return this.schemasState.schemaName;
    }
}

@Injectable()
export class ManualContentsState extends ContentsStateBase {
    public schema: SchemaDto;

    constructor(
        appsState: AppsState, authState: AuthService, contentsService: ContentsService, dialogs: DialogService
    ) {
        super(appsState, authState, contentsService, dialogs);
    }

    protected get schemaName() {
        return this.schema.name;
    }
}

const changeScheduleStatus = (content: ContentDto, status: string, dueTime: string, user: string, version: Version, now?: DateTime) =>
    content.with({
        scheduleJob: new ScheduleDto(status, user, DateTime.parseISO_UTC(dueTime)),
        lastModified: now || DateTime.now(),
        lastModifiedBy: user,
        version
    });

const changeStatus = (content: ContentDto, status: string, user: string, version: Version, now?: DateTime) =>
    content.with({
        status,
        scheduleJob: null,
        lastModified: now || DateTime.now(),
        lastModifiedBy: user,
        version
    });

const updateData = (content: ContentDto, data: any, user: string, version: Version, now?: DateTime) =>
    content.with({
        data,
        dataDraft: data,
        lastModified: now || DateTime.now(),
        lastModifiedBy: user,
        version
    });

const updateDataDraft = (content: ContentDto, data: any, user: string, version: Version, now?: DateTime) =>
    content.with({
        isPending: true,
        dataDraft: data,
        lastModified: now || DateTime.now(),
        lastModifiedBy: user,
        version
    });

const confirmChanges = (content: ContentDto, user: string, version: Version, now?: DateTime) =>
    content.with({
        isPending: false,
        data: content.dataDraft,
        lastModified: now || DateTime.now(),
        lastModifiedBy: user,
        version
    });

const discardChanges = (content: ContentDto, user: string, version: Version, now?: DateTime) =>
    content.with({
        isPending: false,
        dataDraft: content.data,
        lastModified: now || DateTime.now(),
        lastModifiedBy: user,
        version
    });
