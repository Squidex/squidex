/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';
import { FromNowPipe, ScriptLogDto, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-script-log',
    styleUrls: ['./script-log.component.scss'],
    templateUrl: './script-log.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FromNowPipe,
        TranslatePipe,
    ],
})
export class ScriptLogComponent {
    /** The script log to show, including its log entries. */
    public readonly scriptLog = input.required<ScriptLogDto>();

    protected readonly lines = computed(() => {
        return this.scriptLog().entries.map(x => ({
            time: x.timestamp?.toStringFormat('yyyy-MM-dd HH:mm:ss.SSS') ?? '',
            level: x.level,
            message: x.message,
        }));
    });

    protected readonly omitted = computed(() => {
        const scriptLog = this.scriptLog();

        return Math.max(0, scriptLog.totalEntries - scriptLog.entries.length);
    });

    protected readonly isExpanded = signal(false);

    protected toggleExpanded() {
        this.isExpanded.update(x => !x);
    }
}
