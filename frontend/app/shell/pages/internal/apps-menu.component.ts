/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

// tslint:disable: readonly-array

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AppDto, AppsState, DialogModel, fadeAnimation, ModalModel, Title, TitleService, UIState } from '@app/shared';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

@Component({
    selector: 'sqx-apps-menu',
    styleUrls: ['./apps-menu.component.scss'],
    templateUrl: './apps-menu.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppsMenuComponent {
    public addAppDialog = new DialogModel();

    public appsMenu = new ModalModel();
    public appPath: Observable<ReadonlyArray<Title>>;

    constructor(titleService: TitleService,
        public readonly appsState: AppsState,
        public readonly route: ActivatedRoute,
        public readonly uiState: UIState,
    ) {
        this.appPath = titleService.pathChanges.pipe(map(x => x.slice(1)));
    }

    public trackByApp(_index: number, app: AppDto) {
        return app.id;
    }

    public trackByTitle(_index: number, title: Title) {
        return title.value;
    }
}
