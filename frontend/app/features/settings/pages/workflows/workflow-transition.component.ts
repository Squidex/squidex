/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RoleDto, WorkflowTransitionValues, WorkflowTransitionView } from '@app/shared';

@Component({
    selector: 'sqx-workflow-transition[roles][transition]',
    styleUrls: ['./workflow-transition.component.scss'],
    templateUrl: './workflow-transition.component.html',
})
export class WorkflowTransitionComponent {
    public readonly onBlur: { updateOn: 'blur' } = { updateOn: 'blur' };

    @Output()
    public update = new EventEmitter<WorkflowTransitionValues>();

    @Output()
    public remove = new EventEmitter();

    @Input()
    public transition: WorkflowTransitionView;

    @Input()
    public roles: ReadonlyArray<string>;

    @Input()
    public disabled?: boolean | null;

    public changeExpression(expression: string) {
        this.update.emit({ expression });
    }

    public changeRole(roles: ReadonlyArray<string>) {
        this.update.emit(({ roles: roles || [] }) as any);
    }

    public trackByRole(_index: number, role: RoleDto) {
        return role.name;
    }
}
