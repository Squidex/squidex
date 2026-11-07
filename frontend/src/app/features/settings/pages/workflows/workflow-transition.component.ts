/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TagEditorComponent, TranslatePipe, WorkflowTransitionValues, WorkflowTransitionView, WorkflowView } from '@app/shared';

@Component({
    selector: 'sqx-workflow-transition',
    styleUrls: ['./workflow-transition.component.scss'],
    templateUrl: './workflow-transition.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormsModule,
        TagEditorComponent,
        TranslatePipe,
    ],
})
export class WorkflowTransitionComponent {
    public readonly onBlur: { updateOn: 'blur' } = { updateOn: 'blur' };

    /** Emits the updated workflow when the transition is changed or removed. */
    public readonly update = output<WorkflowView>();

    /** The workflow the transition belongs to. */
    public readonly workflow = input.required<WorkflowView>();

    /** The transition to show and edit. */
    public readonly transition = input.required<WorkflowTransitionView>();

    /** The roles that can be selected for the transition. */
    public readonly roles = input.required<ReadonlyArray<string>>();

    /** Prevents the transition from being changed. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    public changeExpression(expression: string) {
        this.change({ expression });
    }

    public changeRole(roles: string[]) {
        this.change({ roles });
    }

    public remove() {
        const { from, to } = this.transition();
        this.update.emit(this.workflow().removeTransition(from, to));
    }

    private change(changes: Partial<WorkflowTransitionValues>) {
        const { from, to } = this.transition();
        this.update.emit(this.workflow().setTransition(from, to, changes));
    }
}
