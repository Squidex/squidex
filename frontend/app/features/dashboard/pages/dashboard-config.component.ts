/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { AppDto, AppsState, AuthService, DialogModel, DialogService, fadeAnimation, LocalizerService, ModalModel, Types, UIState } from '@app/shared';
import { GridsterItem } from 'angular-gridster2';
import { take } from 'rxjs/operators';

@Component({
    selector: 'sqx-dashboard-config',
    styleUrls: ['./dashboard-config.component.scss'],
    templateUrl: './dashboard-config.component.html',
    animations: [
        fadeAnimation,
    ],
})
export class DashboardConfigComponent implements OnChanges {
    @Input()
    public app: AppDto;

    @Input()
    public config: GridsterItem[];

    @Input()
    public needsAttention?: boolean | null;

    @Output()
    public configChange = new EventEmitter<GridsterItem[]>();

    public configOptions: ReadonlyArray<GridsterItem>;

    public expertDialog = new DialogModel();
    public expertConfig: GridsterItem[];

    public dropdownModal = new ModalModel();

    constructor(
        public readonly appsState: AppsState,
        public readonly authState: AuthService,
        private readonly dialogs: DialogService,
        private readonly uiState: UIState,
        localizer: LocalizerService,
    ) {
        this.configOptions =
            [...OPTIONAL_CARDS, ...DEFAULT_CONFIG].map(item => {
                const name = localizer.getOrKey(item.name);

                return { ...item, name };
            }).sortByString(x => x.name);
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['app']) {
            this.uiState.getUser('dashboard.grid', DEFAULT_CONFIG).pipe(take(1))
                .subscribe(dto => {
                    this.setConfig(dto);
                });
        }
    }

    private setConfig(config: any) {
        if (!Types.isArrayOfObject(config)) {
            config = DEFAULT_CONFIG;
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
        this.setConfig(Types.clone(DEFAULT_CONFIG));

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

const DEFAULT_CONFIG: GridsterItem[] = [
    // Row 1
    { cols: 1, rows: 1, x: 0, y: 0, name: 'i18n:dashboard.schemasCard', type: 'schemas' },
    { cols: 1, rows: 1, x: 1, y: 0, name: 'i18n:dashboard.apiDocumentationCard', type: 'api' },
    { cols: 1, rows: 1, x: 2, y: 0, name: 'i18n:dashboard.supportCard', type: 'support' },
    { cols: 1, rows: 1, x: 3, y: 0, name: 'i18n:dashboard.githubCard', type: 'github' },

    // Row 2
    { cols: 2, rows: 1, x: 0, y: 1, name: 'i18n:dashboard.apiCallsChart', type: 'api-calls' },
    { cols: 2, rows: 1, x: 2, y: 1, name: 'i18n:dashboard.apiPerformanceChart', type: 'api-performance' },

    // Row 3
    { cols: 1, rows: 1, x: 0, y: 2, name: 'i18n:dashboard.apiCallsSummaryCard', type: 'api-calls-summary' },
    { cols: 2, rows: 1, x: 1, y: 2, name: 'i18n:dashboard.assetUpdloadsCountChart', type: 'asset-uploads-count' },
    { cols: 1, rows: 1, x: 2, y: 2, name: 'i18n:dashboard.assetUploadsSizeChart', type: 'asset-uploads-size-summary' },

    // Row 4
    { cols: 2, rows: 1, x: 0, y: 3, name: 'i18n:dashboard.assetTotalSize', type: 'asset-uploads-size' },
    { cols: 2, rows: 1, x: 2, y: 3, name: 'i18n:dashboard.trafficChart', type: 'api-traffic' },

    // Row 5
    { cols: 1, rows: 1, x: 0, y: 4, name: 'i18n:dashboard.trafficSummaryCard', type: 'api-traffic-summary' },
    { cols: 2, rows: 1, x: 1, y: 4, name: 'i18n:dashboard.historyCard', type: 'history' },
];

const OPTIONAL_CARDS = [
    { cols: 1, rows: 1, x: 0, y: 0, name: 'i18n:dashboard.randomCatCard', type: 'random-cat' },
    { cols: 1, rows: 1, x: 0, y: 0, name: 'i18n:dashboard.randomDogCard', type: 'random-dog' },
    { cols: 1, rows: 1, x: 0, y: 0, name: 'i18n:dashboard.contentSummaryCard', type: 'content-summary' },
];
