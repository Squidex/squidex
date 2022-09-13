/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { GridsterItem } from 'angular-gridster2';
import { take } from 'rxjs/operators';
import { AppDto, AppsState, AuthService, DialogModel, DialogService, LocalizerService, ModalModel, Types, UIState } from '@app/shared';

@Component({
    selector: 'sqx-dashboard-config[app][config]',
    styleUrls: ['./dashboard-config.component.scss'],
    templateUrl: './dashboard-config.component.html',
})
export class DashboardConfigComponent implements OnChanges {
    @Input()
    public app!: AppDto;

    @Input()
    public config!: GridsterItem[];

    @Input()
    public configDefaults!: GridsterItem[];

    @Input()
    public configAvailable!: GridsterItem[];

    @Input()
    public needsAttention?: boolean | null;

    @Output()
    public configChange = new EventEmitter<GridsterItem[]>();

    public configOptions: ReadonlyArray<GridsterItem> = [];

    public expertDialog = new DialogModel();
    public expertConfig?: GridsterItem[];

    public dropdownModal = new ModalModel();

    constructor(
        public readonly appsState: AppsState,
        public readonly authState: AuthService,
        private readonly localizer: LocalizerService,
        private readonly dialogs: DialogService,
        private readonly uiState: UIState,
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['configAvailable']) {
            this.configOptions = this.configAvailable.map(item => {
                const name = this.localizer.getOrKey(item.name);

                return { ...item, name };
            }).sortByString(x => x.name);
        }

        if (changes['app']) {
            this.uiState.getUser('dashboard.grid', this.configDefaults).pipe(take(1))
                .subscribe(dto => {
                    this.setConfig(dto);
                });
        }
    }

    private setConfig(config: any) {
        if (!Types.isArrayOfObject(config)) {
            config = this.configDefaults;
        }

        this.configChange.emit(Types.clone(config));
    }

    public startExpertMode() {
        this.dropdownModal.hide();

        this.expertConfig = Types.clone(this.config);
        this.expertDialog.show();
    }

    public completeExpertMode() {
        this.setConfig(this.expertConfig);

        this.expertConfig = null!;
        this.expertDialog.hide();
    }

    public resetConfig() {
        this.setConfig(Types.clone(this.configDefaults));

        this.saveConfig();
    }

    public saveConfig() {
        this.uiState.set('dashboard.grid', this.config, true);

        this.dialogs.notifyInfo('i18n:dashboard.configSaved');
    }

    public addOrRemove(item: GridsterItem) {
        const current = this.config.find(x => x.type === item.type);

        if (current) {
            this.config.splice(this.config.indexOf(current), 1);
        } else {
            let y = 0;

            for (const item of this.config) {
                y = Math.max(item.y, y);
            }

            const newOption = Types.clone(item);
            newOption.x = 0;
            newOption.y = y + 1;

            this.config.push(newOption);
        }
    }

    public isSelected(item: GridsterItem) {
        return this.config.find(x => x.type === item.type);
    }
}
