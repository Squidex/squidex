/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, Component, computed, input, output, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { GridsterItemConfig } from 'angular-gridster2';
import { switchMap, take } from 'rxjs/operators';
import { AppDto, AppsState, AuthService, CodeEditorComponent, ConfirmClickDirective, DialogModel, DialogService, DropdownMenuComponent, LocalizerService, ModalDialogComponent, ModalDirective, ModalModel, ModalPlacementDirective, TooltipDirective, TranslatePipe, Types, UIState } from '@app/shared';

@Component({
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
    /** The app of the dashboard. The stored grid configuration is loaded when it changes. */
    public readonly app = input.required<AppDto>();

    /** The current grid configuration. Selecting or unselecting cards changes it in place. */
    public readonly config = input.required<GridsterItemConfig[]>();

    /** The grid configuration to use when nothing is stored or the user resets it. */
    public readonly configDefaults = input.required<GridsterItemConfig[]>();

    /** All cards the user can add to the dashboard. */
    public readonly configAvailable = input.required<GridsterItemConfig[]>();

    /** Highlights the settings button, for example when the dashboard is scrolled. */
    public readonly needsAttention = input(false, { transform: booleanAttribute });

    /** Emits the new grid configuration when it is loaded, reset or edited in expert mode. */
    public readonly configChange = output<GridsterItemConfig[]>();

    public readonly configOptions = computed<ReadonlyArray<GridsterItemConfig>>(() => {
        return this.configAvailable().map(item => {
            const name = this.localizer.getOrKey(item.name);

            return { ...item, name };
        }).sortByString(x => x.name);
    });

    public readonly expertDialog = new DialogModel();
    public readonly expertConfig = signal<GridsterItemConfig[] | undefined>(undefined);

    public readonly dropdownModal = new ModalModel();

    constructor(
        public readonly appsState: AppsState,
        public readonly authState: AuthService,
        private readonly localizer: LocalizerService,
        private readonly dialogs: DialogService,
        private readonly uiState: UIState,
    ) {
        toObservable(this.app).pipe(
            switchMap(() => this.uiState.getAppShared('dashboard.grid', this.configDefaults()).pipe(take(1))),
            takeUntilDestroyed())
            .subscribe(dto => {
                this.setConfig(dto);
            });
    }

    private setConfig(config: any) {
        if (!Types.isArrayOfObject(config)) {
            config = this.configDefaults();
        }

        this.configChange.emit(Types.clone(config));
    }

    public startExpertMode() {
        this.dropdownModal.hide();

        this.expertConfig.set(Types.clone(this.config()));
        this.expertDialog.show();
    }

    public completeExpertMode() {
        this.setConfig(this.expertConfig());

        this.expertConfig.set(undefined);
        this.expertDialog.hide();
    }

    public resetConfig() {
        this.setConfig(Types.clone(this.configDefaults()));
        this.saveConfig();
    }

    public saveConfig() {
        this.uiState.setAppShared('dashboard.grid', this.config());

        this.dialogs.notifyInfo('i18n:dashboard.configSaved');
    }

    public addOrRemove(item: GridsterItemConfig) {
        const config = this.config();
        const current = config.find(x => x.type === item.type);

        if (current) {
            config.splice(config.indexOf(current), 1);
        } else {
            let y = 0;

            for (const item of config) {
                y = Math.max(item.y, y);
            }

            const newOption = Types.clone(item);
            newOption.x = 0;
            newOption.y = y + 1;

            config.push(newOption);
        }
    }

    public isSelected(item: GridsterItemConfig) {
        return this.config().find(x => x.type === item.type);
    }
}
