/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectorRef, Directive, Input, OnChanges, OnInit, TemplateRef, ViewContainerRef } from '@angular/core';

import {
    AppDto,
    AppsState,
    AuthService,
    Permission,
    ResourceOwner,
    SchemaDto,
    SchemasState
} from '@app/shared/internal';

@Directive({
    selector: '[sqxPermission]'
})
export class PermissionDirective extends ResourceOwner implements OnChanges, OnInit {
    private viewCreated = false;

    @Input('sqxPermissionApp')
    public app?: AppDto;

    @Input('sqxPermissionSchema')
    public schema?: SchemaDto;

    @Input('sqxPermission')
    public permissions: string;

    constructor(
        private readonly authService: AuthService,
        private readonly appsState: AppsState,
        private readonly changeDetector: ChangeDetectorRef,
        private readonly schemasState: SchemasState,
        private readonly templateRef: TemplateRef<any>,
        private readonly viewContainer: ViewContainerRef
    ) {
        super();
    }

    public ngOnInit() {
        this.own(
            this.appsState.selectedApp.subscribe(app => {
                if (app && !this.app) {
                    this.update(app, this.schemasState.snapshot.selectedSchema);
                }
            }));

        this.own(
            this.schemasState.selectedSchema.subscribe(schema => {
                if (schema && !this.schema) {
                    this.update(this.appsState.snapshot.selectedApp, schema);
                }
            }));
    }

    public ngOnChanges() {
        this.update(this.appsState.snapshot.selectedApp, this.schemasState.snapshot.selectedSchema);
    }

    private update(app?: AppDto | null, schema?: SchemaDto | null) {
        if (this.app) {
            app = this.app;
        }

        if (this.schema) {
            schema = this.schema;
        }

        let permissions = this.permissions;

        let show = false;

        if (permissions) {
            let include = permissions[0] === '?';

            if (include) {
                permissions = permissions.substr(1);
            }

            const array = permissions.split(';');

            for (let id of array) {
                if (app) {
                    id = id.replace('{app}', app.name);
                }

                if (schema) {
                    id = id.replace('{name}', schema.name);
                }

                const permission = new Permission(id);

                if (include) {
                    if (app && permission.includedIn(app.permissions)) {
                        show = true;
                    }

                    if (!show) {
                        show = permission.includedIn(this.authService.user!.permissions);
                    }
                } else {
                    if (app && permission.allowedBy(app.permissions)) {
                        show = true;
                    }

                    if (!show) {
                        show = permission.allowedBy(this.authService.user!.permissions);
                    }
                }

                if (show) {
                    break;
                }
            }
        }

        if (show && !this.viewCreated) {
            this.viewContainer.createEmbeddedView(this.templateRef);
            this.viewCreated = true;
        } else if (!show && this.viewCreated) {
            this.viewContainer.clear();
            this.viewCreated = false;
        }

        this.changeDetector.markForCheck();
    }
}