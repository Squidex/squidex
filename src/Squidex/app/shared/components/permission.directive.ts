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
    permissionsAllow,
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
        let show = false;

        if (this.permissions) {
            for (let id of this.permissions.split(';')) {
                const app = this.app || this.appsState.snapshot.selectedApp;

                if (app) {
                    id = id.replace('{app}', app.name);
                }

                const schema = this.schema || this.schemasState.snapshot.selectedSchema;

                if (schema) {
                    id = id.replace('{name}', schema.name);
                }

                const permission = new Permission(id);

                if (app && permissionsAllow(app.permissions, permission)) {
                    show = true;
                }

                if (!show) {
                    show = permissionsAllow(this.authService.user!.permissions, permission);
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