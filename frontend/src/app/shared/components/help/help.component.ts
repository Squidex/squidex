/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { LayoutComponent } from '@app/framework';
import { HelpService } from '@app/shared/internal';
import { HelpMarkdownPipe } from './help-markdown.pipe';

@Component({
    standalone: true,
    selector: 'sqx-help',
    styleUrls: ['./help.component.scss'],
    templateUrl: './help.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        HelpMarkdownPipe,
        LayoutComponent,
    ],
})
export class HelpComponent {
    public helpMarkdown = this.helpService.getHelp(this.route.snapshot.data.helpPage);

    constructor(
        private readonly helpService: HelpService,
        private readonly route: ActivatedRoute,
    ) {
    }
}
