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
    notify,
    Pager,
    State,
    Version,
    Versioned
} from '@app/framework';

import { AuthService } from './../services/auth.service';
import { SchemaDto } from './../services/schemas.service';
import { AppsState } from './apps.state';
import { SchemasState } from './schemas.state';

import { ContentDto, ContentsService, ScheduleDto } from './../services/contents.service';

interface Snapshot {
    contents: ImmutableArray<ContentDto>;
    contentsPager: Pager;
    contentsQuery?: string;

    isLoaded?: boolean;
    isArchive?: boolean;

    selectedContent?: ContentDto | null;
}

export abstract class ContentsStateBase extends State<Snapshot> {
    public selectedContent =
        this.changes.pipe(map(x => x.selectedContent),
            distinctUntilChanged());

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
        this.changes.pipe(map(x => !!x.isArchive),
            distinctUntilChanged());

    constructor(
        private readonly appsState: AppsState,
        private readonly authState: AuthService,
        private readonly contentsService: ContentsService,
        private readonly dialogs: DialogService
    ) {
        super({ contents: ImmutableArray.of(), contentsPager: new Pager(0) });
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

    private loadInternal(isReload = false): Observable<any> {
        return this.contentsService.getContents(this.appName, this.schemaName,
                this.snapshot.contentsPager.pageSize,
                this.snapshot.contentsPager.skip,
                this.snapshot.contentsQuery, undefined,
                this.snapshot.isArchive).pipe(
            tap(dtos => {
                if (isReload) {
                    this.dialogs.notifyInfo('Contents reloaded.');
                }

                return this.next(s => {
                    const contents = ImmutableArray.of(dtos.items);
                    const contentsPager = s.contentsPager.setCount(dtos.total);

                    let selectedContent = s.selectedContent;

                    if (selectedContent) {
                        selectedContent = contents.find(x => x.id === selectedContent!.id) || selectedContent;
                    }

                    return { ...s, contents, contentsPager, selectedContent, isLoaded: true };
                });
            }),
            notify(this.dialogs));
    }

    public create(request: any, publish: boolean, now?: DateTime) {
        return this.contentsService.postContent(this.appName, this.schemaName, request, publish).pipe(
            tap(dto => {
                this.dialogs.notifyInfo('Contents created successfully.');

                return this.next(s => {
                    const contents = s.contents.pushFront(dto);
                    const contentsPager = s.contentsPager.incrementCount();

                    return { ...s, contents, contentsPager };
                });
            }),
            notify(this.dialogs));
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
            switchMap(() => this.loadInternal()));
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
            switchMap(() => this.loadInternal()));
    }

    public publishChanges(content: ContentDto, dueTime: string | null, now?: DateTime): Observable<any> {
        return this.contentsService.changeContentStatus(this.appName, this.schemaName, content.id, 'Publish', dueTime, content.version).pipe(
            tap(dto => {
                this.dialogs.notifyInfo('Content updated successfully.');

                if (dueTime) {
                    this.replaceContent(changeScheduleStatus(content, 'Published', dueTime, this.user, dto.version, now));
                } else {
                    this.replaceContent(confirmChanges(content, this.user, dto.version, now));
                }
            }),
            notify(this.dialogs));
    }

    public changeStatus(content: ContentDto, action: string, status: string, dueTime: string | null, now?: DateTime): Observable<any> {
        return this.contentsService.changeContentStatus(this.appName, this.schemaName, content.id, action, dueTime, content.version).pipe(
            tap(dto => {
                this.dialogs.notifyInfo('Content updated successfully.');

                if (dueTime) {
                    this.replaceContent(changeScheduleStatus(content, status, dueTime, this.user, dto.version, now));
                } else {
                    this.replaceContent(changeStatus(content, status, this.user, dto.version, now));
                }
            }),
            notify(this.dialogs));
    }

    public update(content: ContentDto, request: any, now?: DateTime): Observable<any> {
        return this.contentsService.putContent(this.appName, this.schemaName, content.id, request, false, content.version).pipe(
            tap(dto => {
                this.dialogs.notifyInfo('Content updated successfully.');

                if (dto.version.value !== content.version.value) {
                    this.replaceContent(updateData(content, dto.payload, this.user, dto.version, now));
                }
            }),
            notify(this.dialogs));
    }

    public proposeUpdate(content: ContentDto, request: any, now?: DateTime): Observable<any> {
        return this.contentsService.putContent(this.appName, this.schemaName, content.id, request, true, content.version).pipe(
            tap(dto => {
                this.dialogs.notifyInfo('Content updated successfully.');

                if (dto.version.value !== content.version.value) {
                    this.replaceContent(updateDataDraft(content, dto.payload, this.user, dto.version, now));
                }
            }),
            notify(this.dialogs));
    }

    public discardChanges(content: ContentDto, now?: DateTime): Observable<any> {
        return this.contentsService.discardChanges(this.appName, this.schemaName, content.id, content.version).pipe(
            tap(dto => {
                this.dialogs.notifyInfo('Content updated successfully.');

                if (dto.version.value !== content.version.value) {
                    this.replaceContent(discardChanges(content, this.user, dto.version, now));
                }
            }),
            notify(this.dialogs));
    }

    public patch(content: ContentDto, request: any, now?: DateTime): Observable<any> {
        return this.contentsService.patchContent(this.appName, this.schemaName, content.id, request, content.version).pipe(
            tap(dto => {
                this.dialogs.notifyInfo('Content updated successfully.');

                if (dto.version.value !== content.version.value) {
                    this.replaceContent(updateData(content, dto.payload, this.user, dto.version, now));
                }
            }),
            notify(this.dialogs));
    }

    private replaceContent(content: ContentDto) {
        return this.next(s => {
            const contents = s.contents.replaceBy('id', content);
            const selectedContent = s.selectedContent && s.selectedContent.id === content.id ? content : s.selectedContent;

            return { ...s, contents, selectedContent };
        });
    }

    public goArchive(isArchive: boolean): Observable<any> {
        this.next(s => ({ ...s, contentsPager: new Pager(0), contentsQuery: undefined, isArchive }));

        return this.loadInternal();
    }

    public init(): Observable<any> {
        this.next(s => ({ ...s, contentsPager: new Pager(0), contentsQuery: '', isArchive: false, isLoaded: false }));

        return this.loadInternal();
    }

    public search(query: string): Observable<any> {
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
        return this.contentsService.getVersionData(this.appName, this.schemaName, content.id, version).pipe(notify(this.dialogs));
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
