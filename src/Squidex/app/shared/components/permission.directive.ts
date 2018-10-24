/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Directive, Input, OnChanges, TemplateRef, ViewContainerRef } from '@angular/core';

import {
    AppDto,
    AppsState,
    AuthService,
    Permission,
    SchemaDto,
    SchemasState
} from '@app/shared/internal';

@Directive({
    selector: '[sqxPermission]'
})
export class PermissionDirective implements OnChanges {
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
        private readonly schemasState: SchemasState,
        private readonly templateRef: TemplateRef<any>,
        private readonly viewContainer: ViewContainerRef
    ) {
    }

    public ngOnChanges() {
        let permissions = this.permissions;
        let show = false;

        if (permissions) {
            let include = permissions[0] === '?';

            if (include) {
                permissions = permissions.substr(1);
            }

            const array = permissions.split(';');

            for (let id of array) {
                const app = this.app || this.appsState.snapshot.selectedApp;

                if (app) {
                    id = id.replace('{app}', app.name);
                }

                const schema = this.schema || this.schemasState.snapshot.selectedSchema;

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
        } else if (show && this.viewCreated) {
            this.viewContainer.clear();
            this.viewCreated = false;
        }
    }
}