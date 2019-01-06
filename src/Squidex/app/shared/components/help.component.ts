/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgxMdService } from 'ngx-md';

import { HelpService } from '@app/shared/internal';

@Component({
    selector: 'sqx-help',
    styleUrls: ['./help.component.scss'],
    templateUrl: './help.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class HelpComponent {
    public helpMarkdown = this.helpService.getHelp(this.route.snapshot.data['helpPage']);

    constructor(
        private readonly helpService: HelpService,
        private readonly markdownService: NgxMdService,
        private readonly route: ActivatedRoute
    ) {
        this.markdownService.renderer.link = (href, title, text) => {
            return `<a href="https://docs.squidex.io/${href}" title="${title}" target="_blank", rel="noopener">${text} <i class="icon-external-link"></i></a>`;
        };
    }
}