/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { AppDto, AppsState, Settings } from '@app/shared';

@Component({
    selector: 'sqx-left-menu',
    styleUrls: ['./left-menu.component.scss'],
    templateUrl: './left-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class LeftMenuComponent {
    constructor(
        public readonly appsState: AppsState
    ) {
    }

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