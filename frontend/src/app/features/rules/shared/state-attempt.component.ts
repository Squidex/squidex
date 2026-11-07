/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CodeEditorComponent, FlowExecutionStepAttemptDto, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-state-attempt',
    styleUrls: ['./state-attempt.component.scss'],
    templateUrl: './state-attempt.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeEditorComponent,
        FormsModule,
        TranslatePipe,
    ],
})
export class StateAttemptComponent {
    /** The execution attempt to show the times, error and log for. */
    public readonly attempt = input.required<FlowExecutionStepAttemptDto>();

    public readonly output = computed(() => {
        let log = '';

        for (const line of this.attempt().log) {
            log += `${line.timestamp.toISOString()}: ${line.message}\n`;

            if (line.dump) {
                log += `${line.dump}\n\n`;
            }
        }

        return log.replace(/[\r\n\s]+$/, '');
    });
}
