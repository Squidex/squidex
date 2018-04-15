/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { AbstractControl, FormControl, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { Observable, Subscription } from 'rxjs';

import { ContentVersionSelected } from './../messages';

import {
    AppContext,
    AppLanguageDto,
    allData,
    CanComponentDeactivate,
    ContentDto,
    ContentsService,
    fieldInvariant,
    SchemaDetailsDto,
    SchemasState,
    Version
} from '@app/shared';

@Component({
    selector: 'sqx-content-page',
    styleUrls: ['./content-page.component.scss'],
    templateUrl: './content-page.component.html',
    providers: [
        AppContext
    ]
})
export class ContentPageComponent implements CanComponentDeactivate, OnDestroy, OnInit {
    private contentVersionSelectedSubscription: Subscription;
    private selectedSchemaSubscription: Subscription;

    public schema: SchemaDetailsDto;

    public content: ContentDto;
    public contentOld: ContentDto | null;
    public contentFormSubmitted = false;
    public contentForm: FormGroup;

    public isNewMode = true;

    public languages: AppLanguageDto[] = [];

    constructor(public readonly ctx: AppContext,
        private readonly contentsService: ContentsService,
        private readonly router: Router,
        private readonly schemasState: SchemasState
    ) {
    }

    public ngOnDestroy() {
        this.contentVersionSelectedSubscription.unsubscribe();
        this.selectedSchemaSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.contentVersionSelectedSubscription =
            this.ctx.bus.of(ContentVersionSelected)
                .subscribe(message => {
                    this.loadVersion(message.version);
                });

        this.selectedSchemaSubscription =
            this.schemasState.selectedSchema
                .subscribe(schema => {
                    const routeData = allData(this.ctx.route);

                    this.setupLanguages(routeData);
                    this.setupContentForm(schema!);
                });

        this.ctx.route.data.map(d => d.content)
            .subscribe((content: ContentDto) => {
                this.reloadContentForm(content);
            });
    }

    public canDeactivate(): Observable<boolean> {
        if (!this.contentForm.dirty || this.isNewMode) {
            return Observable.of(true);
        } else {
            return this.ctx.confirmUnsavedChanges();
        }
    }

    public showLatest() {
        if (this.contentOld) {
            this.content = this.contentOld;
            this.contentOld = null;

            this.reloadContentForm(this.content);
        }
    }

    public saveAndPublish() {
        this.saveContent(true);
    }

    public saveAsDraft() {
        this.saveContent(false);
    }

    private saveContent(publish: boolean) {
        this.contentFormSubmitted = true;

        if (this.contentForm.valid) {
            this.disableContentForm();

            const requestDto = this.contentForm.value;

            if (this.isNewMode) {
                this.contentsService.postContent(this.ctx.appName, this.schema.name, requestDto, publish)
                    .subscribe(dto => {
                        this.content = dto;

                        this.ctx.notifyInfo('Content created successfully.');

                        this.back();
                    }, error => {
                        this.ctx.notifyError(error);

                        this.enableContentForm();
                    });
            } else {
                this.contentsService.putContent(this.ctx.appName, this.schema.name, this.content.id, requestDto, this.content.version)
                    .subscribe(dto => {
                        const content = this.content.update(dto.payload, this.ctx.userToken, dto.version);

                        this.ctx.notifyInfo('Content saved successfully.');

                        this.enableContentForm();
                        this.reloadContentForm(content);
                    }, error => {
                        this.ctx.notifyError(error);

                        this.enableContentForm();
                    });
            }
        } else {
            this.ctx.notifyError('Content element not valid, please check the field with the red bar on the left in all languages (if localizable).');
        }
    }

    private loadVersion(version: number) {
        if (!this.isNewMode && this.content) {
           this.contentsService.getVersionData(this.ctx.appName, this.schema.name, this.content.id, new Version(version.toString()))
                .subscribe(dto => {
                    if (this.content.version.value !== version.toString()) {
                        this.contentOld = this.content;
                    } else {
                        this.contentOld = null;
                    }

                    this.ctx.notifyInfo('Content version loaded successfully.');

                    this.reloadContentForm(this.content.setData(dto));
                }, error => {
                    this.ctx.notifyError(error);
                });
        }
    }

    public back() {
        this.router.navigate([this.schema.name], { relativeTo: this.ctx.route.parent!.parent, replaceUrl: true });
    }

    private disableContentForm() {
        this.contentForm.disable();
    }

    private enableContentForm() {
        this.contentForm.markAsPristine();

        if (this.schema.fields.length === 0) {
            this.contentForm.enable();
        } else {
            for (const field of this.schema.fields) {
                const fieldForm = <FormGroup>this.contentForm.controls[field.name];

                if (field.isDisabled) {
                    fieldForm.disable();
                } else {
                    fieldForm.enable();
                }
            }
        }
    }

    private setupLanguages(routeData: { [name: string]: any; }) {
        this.languages = routeData.appLanguages;
    }

    private setupContentForm(schema: SchemaDetailsDto) {
        this.schema = schema;

        const controls: { [key: string]: AbstractControl } = {};

        for (const field of schema.fields) {
            const fieldForm = new FormGroup({});

            if (field.isLocalizable) {
                for (let language of this.languages) {
                    fieldForm.setControl(language.iso2Code, new FormControl(undefined, field.createValidators(language.isOptional)));
                }
            } else {
                fieldForm.setControl(fieldInvariant, new FormControl(undefined, field.createValidators(false)));
            }

            controls[field.name] = fieldForm;
        }

        this.contentForm = new FormGroup(controls);

        this.enableContentForm();
    }

    private reloadContentForm(content: ContentDto) {
        this.content = content;
        this.contentForm.markAsPristine();

        this.isNewMode = !this.content;

        if (!this.isNewMode) {
            for (const field of this.schema.fields) {
                const fieldValue = this.content.data[field.name] || {};
                const fieldForm = <FormGroup>this.contentForm.controls[field.name];

                if (field.isLocalizable) {
                    for (let language of this.languages) {
                        fieldForm.controls[language.iso2Code].setValue(fieldValue[language.iso2Code]);
                    }
                } else {
                    fieldForm.controls[fieldInvariant].setValue(fieldValue[fieldInvariant] === undefined ? null : fieldValue[fieldInvariant]);
                }
            }
            if (this.content.status === 'Archived') {
                this.contentForm.disable();
            }
        } else {
            for (const field of this.schema.fields) {
                const defaultValue = field.defaultValue();

                if (defaultValue) {
                    const fieldForm = <FormGroup>this.contentForm.controls[field.name];

                    if (field.isLocalizable) {
                        for (let language of this.languages) {
                            fieldForm.controls[language.iso2Code].setValue(defaultValue);
                        }
                    } else {
                        fieldForm.controls[fieldInvariant].setValue(defaultValue);
                    }
                }
            }
        }
    }
}