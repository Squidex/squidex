/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';

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
export class WorkflowTransitionComponent implements OnChanges {
    @Input()
    public transition: WorkflowTransitionView;

    @Input()
    public roles: RoleDto[];

    @Output()
    public update = new EventEmitter<WorkflowTransitionValues>();

    @Output()
    public remove = new EventEmitter();

    public elementsActive: { [name: string]: boolean } = {};
    public elementsFocused: { [name: string]: boolean } = {};
    public elementsValid: { [name: string]: boolean } = {};

    public onBlur = { updateOn: 'blur' };

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['transition']) {
            if (this.transition.expression) {
                this.elementsValid['expression'] = true;
                this.elementsActive['expression'] = true;
            }

            if (this.transition.role) {
                this.elementsValid['role'] = true;
                this.elementsActive['role'] = true;
            }
        }
    }

    public changeExpression(expression: string) {
        this.update.emit({ expression });
    }

    public changeRole(role: string) {
        this.update.emit({ role });
    }

    public showElement(name: string) {
        this.elementsActive[name] = true;
    }

    public focusElement(name: string) {
        this.elementsFocused[name] = true;
    }

    public blurElement(name: string) {
        this.elementsFocused[name] = false;

        setTimeout(() => {
            if (!this.elementsFocused[name] && !this.elementsValid[name]) {
                this.elementsActive[name] = false;
             }
        }, 2000);
    }

    public trackByRole(index: number, role: RoleDto) {
        return role.name;
    }
}

