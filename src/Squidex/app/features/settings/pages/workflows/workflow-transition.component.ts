/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, Output } from '@angular/core';

import {
    RoleDto,
    WorkflowTransitionValues,
    WorkflowTransitionView
} from '@app/shared';

@Component({
    selector: 'sqx-workflow-transition',
    styleUrls: ['./workflow-transition.component.scss'],
    templateUrl: './workflow-transition.component.html'
})
export class WorkflowTransitionComponent {
    @Input()
    public transition: WorkflowTransitionView;

    @Input()
    public roles: RoleDto[];

    @Input()
    public disabled: boolean;

    @Output()
    public update = new EventEmitter<WorkflowTransitionValues>();

    @Output()
    public remove = new EventEmitter();

    public onBlur = { updateOn: 'blur' };

    public changeExpression(expression: string) {
        this.update.emit({ expression });
    }

    public changeRole(role: string) {
        this.update.emit({ role: role || '' });
    }

    public trackByRole(index: number, role: RoleDto) {
        return role.name;
    }
}

