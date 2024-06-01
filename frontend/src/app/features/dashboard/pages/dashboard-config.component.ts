/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { GridsterItem } from 'angular-gridster2';
import { take } from 'rxjs/operators';
import { AppDto, AppsState, AuthService, CodeEditorComponent, ConfirmClickDirective, DialogModel, DialogService, DropdownMenuComponent, LocalizerService, ModalDialogComponent, ModalDirective, ModalModel, ModalPlacementDirective, TooltipDirective, TranslatePipe, TypedSimpleChanges, Types, UIState } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-dashboard-config',
    styleUrls: ['./dashboard-config.component.scss'],
    templateUrl: './dashboard-config.component.html',
    imports: [
        CodeEditorComponent,
        ConfirmClickDirective,
        DropdownMenuComponent,
        FormsModule,
        ModalDialogComponent,
        ModalDirective,
        ModalPlacementDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class DashboardConfigComponent {
    @Input({ required: true })
    public app!: AppDto;

    @Input({ required: true })
    public config!: GridsterItem[];

    @Input()
    public configDefaults!: GridsterItem[];

    @Input()
    public configAvailable!: GridsterItem[];

    @Input({ transform: booleanAttribute })
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

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.configAvailable) {
            this.configOptions = this.configAvailable.map(item => {
                const name = this.localizer.getOrKey(item.name);

                return { ...item, name };
            }).sortByString(x => x.name);
        }

        if (changes.app) {
            this.uiState.getAppShared('dashboard.grid', this.configDefaults).pipe(take(1))
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
        this.uiState.setAppShared('dashboard.grid', this.config);

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
