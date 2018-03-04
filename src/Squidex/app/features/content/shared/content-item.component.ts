/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnChanges, OnInit, Output } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';

import {
    AppContext,
    AppLanguageDto,
    ContentDto,
    ContentsService,
    fadeAnimation,
    FieldDto,
    fieldInvariant,
    ModalView,
    SchemaDto,
    Types,
    Versioned
} from 'shared';

/* tslint:disable:component-selector */

@Component({
    selector: '[sqxContent]',
    styleUrls: ['./content-item.component.scss'],
    templateUrl: './content-item.component.html',
    providers: [
        AppContext
    ],
    animations: [
        fadeAnimation
    ]
})
export class ContentItemComponent implements OnInit, OnChanges {
    @Output()
    public publishing = new EventEmitter();

    @Output()
    public unpublishing = new EventEmitter();

    @Output()
    public archiving = new EventEmitter();

    @Output()
    public restoring = new EventEmitter();

    @Output()
    public deleting = new EventEmitter();

    @Output()
    public saved = new EventEmitter<Versioned<any>>();

    @Output()
    public selectedChange = new EventEmitter();

    @Input()
    public selected = false;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public schemaFields: FieldDto[];

    @Input()
    public schema: SchemaDto;

    @Input()
    public isReadOnly = false;

    @Input()
    public isReference = false;

    @Input('sqxContent')
    public content: ContentDto;

    public formSubmitted = false;
    public form: FormGroup = new FormGroup({});

    public dropdown = new ModalView(false, true);

    public values: any[] = [];

    constructor(public readonly ctx: AppContext,
        private readonly contentsService: ContentsService
    ) {
    }

    public ngOnChanges() {
        this.updateValues();
    }

    public ngOnInit() {
        for (let field of this.schemaFields) {
            if (field.properties['inlineEditable']) {
                this.form.setControl(field.name, new FormControl(undefined, field.createValidators(this.language.isOptional)));
            }
        }

        this.updateValues();
    }

    public shouldStop(event: Event) {
        if (this.form.dirty) {
            event.stopPropagation();
            event.stopImmediatePropagation();
        }
    }

    public save() {
        this.formSubmitted = true;

        if (this.form.dirty && this.form.valid) {
            this.form.disable();

            const request = {};

            for (let field of this.schemaFields) {
                if (field.properties['inlineEditable']) {
                    const value = this.form.controls[field.name].value;

                    if (field.isLocalizable) {
                        request[field.name] = { [this.language.iso2Code]: value };
                    } else {
                        request[field.name] = { iv: value };
                    }
                }
            }

            this.contentsService.patchContent(this.ctx.appName, this.schema.name, this.content.id, request, this.content.version)
                .finally(() => {
                    this.form.enable();
                })
                .subscribe(dto => {
                    this.form.markAsPristine();

                    this.emitSaved(dto);
                }, error => {
                    this.ctx.notifyError(error);
                });
        }
    }

    private emitSaved(data: Versioned<any>) {
        this.saved.emit(data);
    }

    private updateValues() {
        this.values = [];

        if (this.schemaFields) {
            for (let field of this.schemaFields) {
                const value = this.getRawValue(field);

                if (Types.isUndefined(value)) {
                    this.values.push('');
                } else {
                    this.values.push(field.formatValue(value));
                }

                if (this.form) {
                    const formControl = this.form.controls[field.name];

                    if (formControl) {
                        formControl.setValue(value);
                    }
                }
            }
        }
    }

    private getRawValue(field: FieldDto): any {
        const contentField = this.content.data[field.name];

        if (contentField) {
            if (field.isLocalizable) {
                return contentField[this.language.iso2Code];
            } else {
                return contentField[fieldInvariant];
            }
        }

        return undefined;
    }
}

