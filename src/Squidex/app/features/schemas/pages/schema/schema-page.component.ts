/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs';

import {
    AddFieldDto,
    AppComponentBase,
    AppsStoreService,
    FieldDto,
    FieldPropertiesDto,
    HistoryChannelUpdated,
    ImmutableArray,
    MessageBus,
    NotificationService,
    NumberFieldPropertiesDto,
    SchemaDetailsDto,
    SchemasService,
    StringFieldPropertiesDto,
    UsersProviderService
} from 'shared';

@Component({
    selector: 'sqx-schema-page',
    styleUrls: ['./schema-page.component.scss'],
    templateUrl: './schema-page.component.html'
})
export class SchemaPageComponent extends AppComponentBase implements OnInit {
    public fieldTypes: string[] = [
        'string',
        'number'
    ];

    public schemaFields = ImmutableArray.empty<FieldDto>();

    public addFieldForm: FormGroup =
        this.formBuilder.group({
            type: ['string',
                [
                    Validators.required
                ]],
            name: ['',
                [
                    Validators.required,
                    Validators.maxLength(40),
                    Validators.pattern('[a-z0-9]+(\-[a-z0-9]+)*')
                ]]
        });

    public get schemaName(): Observable<string> {
        return this.route.params.map(p => p['schemaName']);
    }

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly schemasService: SchemasService,
        private readonly messageBus: MessageBus,
        private readonly formBuilder: FormBuilder,
        private readonly route: ActivatedRoute
    ) {
        super(apps, notifications, users);
    }

    public ngOnInit() {
        this.load();
    }

    public load() {
        this.schemaName.combineLatest(this.appName(), (schemaName, appName) => { return { schemaName, appName }; })
            .switchMap(p => this.schemasService.getSchema(p.appName, p.schemaName)).retry(2)
            .subscribe(dto => {
                this.schemaFields = ImmutableArray.of(dto.fields);
            }, error => {
                this.notifyError(error);
            });
    }

    public addField() {
        this.addFieldForm.markAsTouched();

        if (this.addFieldForm.valid) {
            this.addFieldForm.disable();

            let properties: FieldPropertiesDto;

            switch (this.addFieldForm.get('type').value) {
                case 'string':
                    properties = new StringFieldPropertiesDto();
                    break;
                case 'number':
                    properties = new NumberFieldPropertiesDto();
            }

            const dto = new AddFieldDto(this.addFieldForm.get('name').value, properties);

            const reset = () => {
                this.addFieldForm.reset();
                this.addFieldForm.enable();
            };

            this.schemaName.combineLatest(this.appName(), (schemaName, appName) => { return { schemaName, appName }; })
                .switchMap(p => this.schemasService.postField(p.appName, p.schemaName, dto))
                .subscribe(dto => {
                    this.updateFields(this.schemaFields.push(new FieldDto(this.addFieldForm.get('name').value, false, false, properties)));
                    reset();
                }, error => {
                    this.notifyError(error);
                    reset();
                });
        }
    }

    private updateFields(fields: ImmutableArray<FieldDto>) {
        this.schemaFields = fields;

        this.messageBus.publish(new HistoryChannelUpdated());
    }
}

