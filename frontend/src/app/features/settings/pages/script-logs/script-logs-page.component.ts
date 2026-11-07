/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FormAlertComponent, LayoutComponent, ListViewComponent, ScriptLogsState, ShortcutDirective, TitleComponent, TranslatePipe } from '@app/shared';
import { ScriptLogComponent } from './script-log.component';

@Component({
    selector: 'sqx-script-logs-page',
    styleUrls: ['./script-logs-page.component.scss'],
    templateUrl: './script-logs-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormAlertComponent,
        FormsModule,
        LayoutComponent,
        ListViewComponent,
        ScriptLogComponent,
        ShortcutDirective,
        TitleComponent,
        TranslatePipe,
    ],
})
export class ScriptLogsPageComponent {
    constructor(
        protected readonly scriptLogsState: ScriptLogsState,
    ) {
        this.scriptLogsState.load();
    }

    protected reload() {
        this.scriptLogsState.load(true);
    }

    protected filter(name: string) {
        this.scriptLogsState.filter(name);
    }
}
