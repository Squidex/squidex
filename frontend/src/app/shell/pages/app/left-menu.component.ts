/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AppDto, Settings, TourStepDirective, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-left-menu',
    styleUrls: ['./left-menu.component.scss'],
    templateUrl: './left-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        RouterLink,
        RouterLinkActive,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class LeftMenuComponent {
    @Input({ required: true })
    public app!: AppDto;

    public isAssetsHidden(app: AppDto) {
        return app.roleProperties[Settings.AppProperties.HIDE_ASSETS] === true;
    }

    public isSettingsHidden(app: AppDto) {
        return app.roleProperties[Settings.AppProperties.HIDE_SETTINGS] === true;
    }

    public isSchemasHidden(app: AppDto) {
        return app.roleProperties[Settings.AppProperties.HIDE_SCHEMAS] === true;
    }

    public isApiHidden(app: AppDto) {
        return app.roleProperties[Settings.AppProperties.HIDE_API] === true;
    }
}
