/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';

import '@app/framework/utils/rxjs-extensions';

import {
    DateTime,
    DialogService,
    ErrorDto,
    Form,
    ImmutableArray,
    Pager,
    State,
    Version,
    Versioned
} from '@app/framework';

import { AppLanguageDto } from './../services/app-languages.service';
import { AuthService } from './../services/auth.service';
import { fieldInvariant, SchemaDetailsDto, SchemaDto } from './../services/schemas.service';
import { AppsState } from './apps.state';
import { SchemasState } from './schemas.state';

import { ContentDto, ContentsService } from './../services/contents.service';

export class EditContentForm extends Form<FormGroup> {
    constructor(
        private readonly schema: SchemaDetailsDto,
        private readonly languages: ImmutableArray<AppLanguageDto>
    ) {
        super(new FormGroup({}));

        for (const field of schema.fields) {
            const fieldForm = new FormGroup({});

            const defaultValue = field.defaultValue();

            if (field.isLocalizable) {
                for (let language of this.languages.values) {
                    fieldForm.setControl(language.iso2Code, new FormControl(defaultValue, field.createValidators(language.isOptional)));
                }
            } else {
                fieldForm.setControl(fieldInvariant, new FormControl(defaultValue, field.createValidators(false)));
            }

            this.form.setControl(field.name, fieldForm);
        }

        this.enableContentForm();
    }

    public submitCompleted(newValue?: any) {
        super.submitCompleted(newValue);

        this.enableContentForm();
    }

    public submitFailed(error?: string | ErrorDto) {
        super.submitFailed(error);

        this.enableContentForm();
    }

    public loadData(value: any, isArchive: boolean) {
        super.load(value);

        if (isArchive) {
            this.form.disable();
        } else {
            this.enableContentForm();
        }
    }

    private enableContentForm() {
        if (this.schema.fields.length === 0) {
            this.form.enable();
        } else {
            for (const field of this.schema.fields) {
                const fieldForm = this.form.controls[field.name];

                if (field.isDisabled) {
                    fieldForm.disable();
                } else {
                    fieldForm.enable();
                }
            }
        }
    }
}

export class PatchContentForm extends Form<FormGroup> {
    constructor(
        private readonly schema: SchemaDetailsDto,
        private readonly language: AppLanguageDto
    ) {
        super(new FormGroup({}));

        for (let field of this.schema.listFields) {
            if (field.properties && field.properties['inlineEditable']) {
                this.form.setControl(field.name, new FormControl(undefined, field.createValidators(this.language.isOptional)));
            }
        }
    }

    public submit() {
        const result = super.submit();

        if (result) {
            const request = {};

            for (let field of this.schema.listFields) {
                if (field.properties['inlineEditable']) {
                    const value = result[field.name];

                    if (field.isLocalizable) {
                        request[field.name] = { [this.language.iso2Code]: value };
                    } else {
                        request[field.name] = { iv: value };
                    }
                }
            }

            return request;
        }

        return result;
    }
}

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
        this.changes.map(x => x.selectedContent)
            .distinctUntilChanged();

    public contents =
        this.changes.map(x => x.contents)
            .distinctUntilChanged();

    public contentsPager =
        this.changes.map(x => x.contentsPager)
            .distinctUntilChanged();

    public contentsQuery =
        this.changes.map(x => x.contentsQuery)
            .distinctUntilChanged();

    public isLoaded =
        this.changes.map(x => !!x.isLoaded)
            .distinctUntilChanged();

    public isArchive =
        this.changes.map(x => !!x.isArchive)
            .distinctUntilChanged();

    constructor(
        private readonly appsState: AppsState,
        private readonly authState: AuthService,
        private readonly contentsService: ContentsService,
        private readonly dialogs: DialogService
    ) {
        super({ contents: ImmutableArray.of(), contentsPager: new Pager(0) });
    }

    public select(id: string | null): Observable<ContentDto | null> {
        return this.loadContent(id)
            .do(content => {
                this.next(s => {
                    const contents = content ? s.contents.replaceBy('id', content) : s.contents;

                    return { ...s, selectedContent: content, contents };
                });
            });
    }

    private loadContent(id: string | null) {
        return !id ?
            Observable.of(null) :
            Observable.of(this.snapshot.contents.find(x => x.id === id))
                .switchMap(content => {
                    if (!content) {
                        return this.contentsService.getContent(this.appName, this.schemaName, id).catch(() => Observable.of(null));
                    } else {
                        return Observable.of(content);
                    }
                });
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
                this.snapshot.isArchive)
            .do(dtos => {
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
            })
            .notify(this.dialogs);
    }

    public create(request: any, publish: boolean, now?: DateTime) {
        return this.contentsService.postContent(this.appName, this.schemaName, request, publish)
            .do(dto => {
                this.dialogs.notifyInfo('Contents created successfully.');

                return this.next(s => {
                    const contents = s.contents.pushFront(dto);
                    const contentsPager = s.contentsPager.incrementCount();

                    return { ...s, contents, contentsPager };
                });
            })
            .notify(this.dialogs);
    }

    public changeStatus(contents: ContentDto[], action: string, dueTime: string | null): Observable<any> {
        return Observable.forkJoin(
            contents.map(c =>
                this.contentsService.changeContentStatus(this.appName, this.schemaName, c.id, action, dueTime, c.version)
                    .catch(error => Observable.of(error))))
            .do(results => {
                const error = results.find(x => !!x.error);

                if (error) {
                    this.dialogs.notifyError(error);
                }

                return Observable.of(error);
            })
            .switchMap(() => this.loadInternal());
    }

    public delete(contents: ContentDto[]): Observable<any> {
        return Observable.forkJoin(
                contents.map(c =>
                    this.contentsService.deleteContent(this.appName, this.schemaName, c.id, c.version)
                        .catch(error => Observable.of(error))))
            .do(results => {
                const error = results.find(x => !!x.error);

                if (error) {
                    this.dialogs.notifyError(error);
                }

                return Observable.of(error);
            })
            .switchMap(() => this.loadInternal());
    }

    public update(content: ContentDto, request: any, now?: DateTime): Observable<any> {
        return this.contentsService.putContent(this.appName, this.schemaName, content.id, request, content.version)
            .do(dto => {
                this.dialogs.notifyInfo('Contents updated successfully.');

                this.replaceContent(updateData(content, dto.payload, this.user, dto.version, now));
            })
            .notify(this.dialogs);
    }

    public patch(content: ContentDto, request: any, now?: DateTime): Observable<any> {
        return this.contentsService.patchContent(this.appName, this.schemaName, content.id, request, content.version)
            .do(dto => {
                this.dialogs.notifyInfo('Contents updated successfully.');

                this.replaceContent(updateData(content, dto.payload, this.user, dto.version, now));
            })
            .notify(this.dialogs);
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
        return this.contentsService.getVersionData(this.appName, this.schemaName, content.id, new Version(version.toString()))
            .notify(this.dialogs);
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

const updateData = (content: ContentDto, data: any, user: string, version: Version, now?: DateTime) =>
    new ContentDto(
        content.id,
        content.status,
        content.createdBy, user,
        content.created, now || DateTime.now(),
        content.scheduledTo,
        content.scheduledBy,
        content.scheduledAt,
        data,
        version);