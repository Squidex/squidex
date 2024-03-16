/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { map } from 'rxjs/operators';
import { defined, MessageBus, ModalModel, ResourceOwner, SchemaDto, SchemasState } from '@app/shared';
import { SchemaCloning } from './../messages';

@Component({
    selector: 'sqx-schema-page',
    styleUrls: ['./schema-page.component.scss'],
    templateUrl: './schema-page.component.html',
})
export class SchemaPageComponent extends ResourceOwner implements OnInit {
    public readonly exact = { exact: true };

    public schema!: SchemaDto;
    public schemaTab = this.route.queryParams.pipe(map(x => x['tab'] || 'fields'));

    public editOptionsDropdown = new ModalModel();

    constructor(
        public readonly schemasState: SchemasState,
        private readonly route: ActivatedRoute,
        private readonly router: Router,
        private readonly messageBus: MessageBus,
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.schemasState.selectedSchema.pipe(defined())
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

    private back() {
        this.router.navigate(['../'], { relativeTo: this.route });
    }
}
