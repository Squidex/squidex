/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import {
    fadeAnimation,
    MessageBus,
    ModalModel,
    ResourceOwner,
    SchemaDetailsDto,
    SchemasState
} from '@app/shared';

import {
    SchemaCloning
} from './../messages';

@Component({
    selector: 'sqx-schema-page',
    styleUrls: ['./schema-page.component.scss'],
    templateUrl: './schema-page.component.html',
    animations: [
        fadeAnimation
    ]
})
export class SchemaPageComponent extends ResourceOwner implements OnInit {
    public readonly exact = { exact: true };

    public schema: SchemaDetailsDto;

    public selectableTabs: ReadonlyArray<string> = ['Fields', 'Scripts', 'Json', 'More'];
    public selectedTab = this.selectableTabs[0];

    public editOptionsDropdown = new ModalModel();

    constructor(
        public readonly schemasState: SchemasState,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly messageBus: MessageBus
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.schemasState.selectedSchema
                .subscribe(schema => {
                    this.schema = schema;
                }));
    }

    public cloneSchema() {
        this.messageBus.emit(new SchemaCloning(this.schema.export()));
    }

    public publish() {
        this.schemasState.publish(this.schema).subscribe();
    }

    public unpublish() {
        this.schemasState.unpublish(this.schema).subscribe();
    }

    public deleteSchema() {
        this.schemasState.delete(this.schema)
            .subscribe(() => {
                this.back();
            });
    }

    public selectTab(tab: string) {
        this.selectedTab = tab;
    }

    private back() {
        this.router.navigate(['../'], { relativeTo: this.route });
    }
}