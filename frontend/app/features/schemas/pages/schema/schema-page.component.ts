/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable:no-shadowed-variable

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import {
    fadeAnimation,
    MessageBus,
    ModalModel,
    PatternsState,
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
    public schema: SchemaDetailsDto;

    public editOptionsDropdown = new ModalModel();

    public selectedTab = 'More';
    public selectableTabs: ReadonlyArray<string> = ['Fields', 'Scripts', 'Json', 'More'];

    constructor(
        public readonly schemasState: SchemasState,
        public readonly patternsState: PatternsState,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly messageBus: MessageBus
    ) {
        super();
    }

    public ngOnInit() {
        this.patternsState.load();

        this.own(
            this.route.data
                .subscribe(data => {
                    this.selectedTab = data['tab'];
                }));

        this.own(
            this.schemasState.selectedSchema
                .subscribe(schema => {
                    this.schema = schema;
                }));
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
        const hasHelp = this.router.url.endsWith('/help');

        if (hasHelp) {
            this.router.navigate(['../', tab.toLowerCase(), 'help'], { relativeTo: this.route });
        } else {
            this.router.navigate(['../', tab.toLowerCase()], { relativeTo: this.route });
        }
    }

    private back() {
        this.router.navigate(['../'], { relativeTo: this.route });
    }
}