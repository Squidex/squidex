/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, Component, computed, input, output, signal, inject } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { GridsterItemConfig } from 'angular-gridster2';
import { switchMap } from 'rxjs/operators';
import { AppDto, CodeEditorComponent, ConfirmClickDirective, DialogModel, DialogService, DropdownMenuComponent, LocalizerService, ModalDialogComponent, ModalDirective, ModalModel, ModalPlacementDirective, TooltipDirective, TranslatePipe, Types, UIState } from '@app/shared';

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
    private readonly localizer = inject(LocalizerService);
    private readonly dialogs = inject(DialogService);
    private readonly uiState = inject(UIState);

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

    protected readonly configOptions = computed<ReadonlyArray<GridsterItemConfig>>(() => {
        return this.configAvailable().map(item => {
            const name = this.localizer.getOrKey(item.name);

            return { ...item, name };
        }).sortByString(x => x.name);
    });

    protected readonly expertDialog = new DialogModel();
    protected readonly expertConfig = signal<GridsterItemConfig[] | undefined>(undefined);

    protected readonly dropdownModal = new ModalModel();

    constructor() {
        toObservable(this.app).pipe(
            switchMap(() => this.uiState.waitForAppShared('dashboard.grid', this.configDefaults())),
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

    protected startExpertMode() {
        this.dropdownModal.hide();

        this.expertConfig.set(Types.clone(this.config()));
        this.expertDialog.show();
    }

    protected completeExpertMode() {
        this.setConfig(this.expertConfig());

        this.expertConfig.set(undefined);
        this.expertDialog.hide();
    }

    protected resetConfig() {
        this.setConfig(Types.clone(this.configDefaults()));
        this.saveConfig();
    }

    protected saveConfig() {
        this.uiState.setAppShared('dashboard.grid', this.config());

        this.dialogs.notifyInfo('i18n:dashboard.configSaved');
    }

    protected addOrRemove(item: GridsterItemConfig) {
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

    protected isSelected(item: GridsterItemConfig) {
        return this.config().find(x => x.type === item.type);
    }
}
