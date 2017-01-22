/*
 * Squidex Headless CMS
 * 
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';

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

    public schemas = ImmutableArray.empty<SchemaDto>();
    public schemaQuery: string;
    public schemasFilter = new FormControl();
    public schemasFiltered = ImmutableArray.empty<SchemaDto>();

    constructor(apps: AppsStoreService, notifications: NotificationService, users: UsersProviderService,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
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
        this.schemasFilter.valueChanges.distinctUntilChanged().debounceTime(100)
            .subscribe(q => {
                this.router.navigate([], { queryParams: { schemaQuery: q }});
            });

        this.route.queryParams.map(q => q['schemaQuery']).distinctUntilChanged()
            .subscribe(q => {
                this.updateSchemas(this.schemas, this.schemaQuery = q);
            });

        this.messageSubscription =
            this.messageBus.of(SchemaUpdated)
                .subscribe(m => {
                    this.updateSchemas(this.schemas.map(s => s.name === m.name ? updateSchema(s, this.authService, m) : s));
                });

        this.load();
    }

    public load() {
        this.appName()
            .switchMap(app => this.schemasService.getSchemas(app).retry(2))
            .subscribe(dtos => {
                this.updateSchemas(ImmutableArray.of(dtos));
            }, error => {
                this.notifyError(error);
            });
    }

    public onSchemaCreated(dto: SchemaDto) {
        this.updateSchemas(this.schemas.push(dto), this.schemaQuery);

        this.addSchemaDialog.hide();
    }

    private updateSchemas(schemas: ImmutableArray<SchemaDto>, query?: string) {
        this.schemas = schemas;

        query = query || this.schemaQuery;

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

        this.schemasFiltered = schemas;
    }
}

function updateSchema(schema: SchemaDto, authService: AuthService, message: SchemaUpdated): SchemaDto {
    const me = `subject:${authService.user!.id}`;

    return new SchemaDto(
        schema.id,
        schema.name,
        message.label,
        message.isPublished,
        schema.createdBy, me,
        schema.created, DateTime.now());
}


