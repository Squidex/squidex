/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject, Service } from '@angular/core';
import { Observable } from 'rxjs';
import { finalize, tap } from 'rxjs/operators';
import { debug, DialogService, LoadingState, shareSubscribed, State } from '@app/framework';
import { TemplateDto } from '../model';
import { TemplatesService } from '../services/templates.service';

interface Snapshot extends LoadingState {
    // The current templates.
    templates: ReadonlyArray<TemplateDto>;

    // Indicates if the user can create new templates.
    canCreate?: boolean;
}

@Service()
export class TemplatesState extends State<Snapshot> {
    private readonly dialogs = inject(DialogService);
    private readonly templatesService = inject(TemplatesService);

    public readonly templates =
        this.project(x => x.templates);

    public readonly starters =
        this.project(x => x.templates.filter(x => x.isStarter));

    public readonly isLoaded =
        this.project(x => x.isLoaded === true);

    public readonly isLoading =
        this.project(x => x.isLoading === true);

    constructor() {
        super({ templates: [] });

        debug(this, 'templates');
    }

    public load(isReload = false, silent = false): Observable<any> {
        if (isReload) {
            this.resetState('Loading Initial');
        }

        return this.loadInternal(isReload, silent);
    }

    private loadInternal(isReload: boolean, silent: boolean): Observable<any> {
        this.next({ isLoading: true }, 'Loading Started');

        return this.templatesService.getTemplates().pipe(
            tap(({ items: templates }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:templates.reloaded');
                }

                this.next({
                    templates,
                    isLoaded: true,
                    isLoading: false,
                }, 'Loading Success');
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
            }),
            shareSubscribed(this.dialogs, { silent }));
    }
}
