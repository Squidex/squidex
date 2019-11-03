/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:no-shadowed-variable

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';

import {
    fadeAnimation,
    MessageBus,
    ModalModel,
    ResourceOwner,
    SchemaDetailsDto,
    SchemasState,
    Types
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
    public schema: SchemaDetailsDto;

    public editOptionsDropdown = new ModalModel();

    public selectedTab = 'fields';
    public selectableTabs: ReadonlyArray<string> = ['Fields', 'Scripts', 'Json', 'More'];

    constructor(
        public readonly schemasState: SchemasState,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly messageBus: MessageBus
    ) {
        super();
    }

    public ngOnInit() {
        this.updateTab();

        this.own(
            this.router.events
                .subscribe(event => {
                    if (Types.is(event, NavigationEnd)) {
                        this.updateTab();
                    }
                }));

        this.own(
            this.schemasState.selectedSchema
                .subscribe(schema => {
                    this.schema = schema;
                }));
    }

    private updateTab() {
        this.selectedTab = this.route.firstChild!.snapshot.routeConfig!.path!;
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

    public cloneSchema() {
        this.messageBus.emit(new SchemaCloning(this.schema.export()));
    }

    public selectTab(tab: string) {
        this.selectedTab = tab;
    }

    private back() {
        this.router.navigate(['../'], { relativeTo: this.route });
    }
}