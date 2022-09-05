/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnChanges } from '@angular/core';
import { map, Observable, shareReplay } from 'rxjs';
import { AppsState, ClientsState, TemplateDetailsDto, TemplateDto, TemplatesService } from '@app/shared';

@Component({
    selector: 'sqx-template[template]',
    styleUrls: ['./template.component.scss'],
    templateUrl: './template.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TemplateComponent implements OnChanges {
    @Input()
    public template!: TemplateDto;

    public isExpanded = false;

    public details?: Observable<string>;

    constructor(
        private readonly clientsState: ClientsState,
        private readonly appsState: AppsState,
        private readonly templatesService: TemplatesService,
    ) {
    }

    public ngOnChanges() {
        this.details = this.templatesService.getTemplate(this.template).pipe(map(x => this.buildDetails(x)), shareReplay(1));
    }

    public toggleExpanded() {
        this.isExpanded = !this.isExpanded;
    }

    private buildDetails(dto: TemplateDetailsDto) {
        const app = this.appsState.appName;

        let details = dto.details.replace(/<APP>/g, app);

        const client = this.clientsState.snapshot.clients[0];

        if (client) {
            const clientId = `${app}:${client.id}`;

            details = details.replace(/\<CLIENT_ID>/g, clientId);
            details = details.replace(/\<CLIENT_SECRET>/g, client.secret);
        }

        return details;
    }
}
