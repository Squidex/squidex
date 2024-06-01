/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TagEditorComponent, TranslatePipe, WorkflowTransitionValues, WorkflowTransitionView } from '@app/shared';

@Component({
    standalone: true,
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
    public update = new EventEmitter<WorkflowTransitionValues>();

    @Output()
    public remove = new EventEmitter();

    @Input({ required: true })
    public transition!: WorkflowTransitionView;

    @Input({ required: true })
    public roles!: ReadonlyArray<string>;

    @Input({ transform: booleanAttribute })
    public disabled?: boolean | null;

    public changeExpression(expression: string) {
        this.update.emit({ expression });
    }

    public changeRole(roles: ReadonlyArray<string>) {
        this.update.emit(({ roles: roles || [] }) as any);
    }
}
