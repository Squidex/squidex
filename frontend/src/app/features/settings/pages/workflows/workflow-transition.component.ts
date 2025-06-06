/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TagEditorComponent, TranslatePipe, WorkflowTransitionValues, WorkflowTransitionView, WorkflowView } from '@app/shared';

@Component({
    selector: 'sqx-workflow-transition',
    styleUrls: ['./workflow-transition.component.scss'],
    templateUrl: './workflow-transition.component.html',
    imports: [
        FormsModule,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class WorkflowTransitionComponent {
    public readonly onBlur: { updateOn: 'blur' } = { updateOn: 'blur' };

    @Output()
    public update = new EventEmitter<WorkflowView>();

    @Input({ required: true })
    public workflow!: WorkflowView;

    @Input({ required: true })
    public transition!: WorkflowTransitionView;

    @Input({ required: true })
    public roles!: ReadonlyArray<string>;

    @Input({ transform: booleanAttribute })
    public disabled?: boolean | null;

    public changeExpression(expression: string) {
        this.change({ expression });
    }

    public changeRole(roles: string[]) {
        this.change({ roles });
    }

    public remove() {
        const { from, to } = this.transition;
        this.update.emit(this.workflow.removeTransition(from, to));
    }

    private change(changes: Partial<WorkflowTransitionValues>) {
        const { from, to } = this.transition;
        this.update.emit(this.workflow.setTransition(from, to, changes));
    }
}
