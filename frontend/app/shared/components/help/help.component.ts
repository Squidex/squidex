/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { HelpService } from '@app/shared/internal';

@Component({
    selector: 'sqx-help',
    styleUrls: ['./help.component.scss'],
    templateUrl: './help.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HelpComponent {
    public helpMarkdown = this.helpService.getHelp(this.route.snapshot.data.helpPage);

    constructor(
        private readonly helpService: HelpService,
        private readonly route: ActivatedRoute,
    ) {
    }
}
