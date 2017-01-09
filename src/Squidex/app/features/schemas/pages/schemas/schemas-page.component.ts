/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { BehaviorSubject, Observable, Subscription } from 'rxjs';

import {
    AppComponentBase,
    AppsStoreService,
    AuthService,
    DateTime,
    fadeAnimation,
    ImmutableArray,
    MessageBus,
    ModalView,
    NotificationService,
    SchemaDto,
    SchemasService,
    UsersProviderService
} from 'shared';

import { SchemaUpdated } from './../messages';

@Component({
    selector: 'sqx-schemas-page',
    styleUrls: ['./schemas-page.component.scss'],
    templateUrl: './schemas-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class SchemasPageComponent extends AppComponentBase implements OnDestroy, OnInit {
    private messageSubscription: Subscription;

    public addSchemaDialog = new ModalView();

    public schemas = new BehaviorSubject(ImmutableArray.empty<SchemaDto>());
    public schemasFilter = new FormControl();
    public schemasFiltered =
        Observable.of(null)
            .merge(this.schemasFilter.valueChanges.debounceTime(100))
            .combineLatest(this.schemas,
                (query, schemas) => {

                if (query && query.length > 0) {
                    schemas = schemas.filter(t => t.name.indexOf(query) >= 0);
                }

                schemas =
                    schemas.sort((a, b) => {
                        if (a.name < b.name) {
                            return -1;
                        }
                        if (a.name > b.name) {
                            return 1;
                        }
                        return 0;
                    });

                return schemas;
            });

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly schemasService: SchemasService,
        private readonly messageBus: MessageBus,
        private readonly authService: AuthService
    ) {
        super(apps, notifications, users);
    }

    public ngOnDestroy() {
        this.messageSubscription.unsubscribe();
    }

    public ngOnInit() {
        this.load();

        this.messageSubscription =
            this.messageBus.of(SchemaUpdated).subscribe(message => {
                const schemas = this.schemas.value;
                const oldSchema = schemas.find(i => i.name === message.name);

                if (oldSchema) {
                    const me = `subject:${this.authService.user.id}`;

                    const newSchema =
                        new SchemaDto(
                            oldSchema.id,
                            oldSchema.name,
                            message.label,
                            message.isPublished,
                            oldSchema.createdBy, me,
                            oldSchema.created, DateTime.now());
                    this.schemas.next(schemas.replace(oldSchema, newSchema));
                }
            });
    }

    public load() {
        this.appName()
            .switchMap(app => this.schemasService.getSchemas(app).retry(2))
            .subscribe(dtos => {
                this.schemas.next(ImmutableArray.of(dtos));
            }, error => {
                this.notifyError(error);
            });
    }

    public onSchemaCreated(dto: SchemaDto) {
        this.schemas.next(this.schemas.getValue().push(dto));

        this.addSchemaDialog.hide();
    }
}

