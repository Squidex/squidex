/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { FormField } from '@angular/forms/signals';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AppsState, AssetScriptsState, AssetsService, CodeEditorComponent, EditAssetScriptsForm, LayoutComponent, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { ScriptNamePipe } from '@app/shared/components/pipes';

@Component({
    selector: 'sqx-asset-scripts-page',
    styleUrls: ['./asset-scripts-page.component.scss'],
    templateUrl: './asset-scripts-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeEditorComponent,
        FormField,
        FormsModule,
        LayoutComponent,
        RouterLink,
        RouterLinkActive,
        RouterOutlet,
        ScriptNamePipe,
        ShortcutDirective,
        SidebarMenuDirective,
        TitleComponent,
        TooltipDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class AssetScriptsPageComponent {
    private readonly appsState = inject(AppsState);
    private readonly assetScriptsState = inject(AssetScriptsState);
    private readonly assetsService = inject(AssetsService);

    protected readonly assetScript = signal('query');

    protected readonly isEditable = this.assetScriptsState.canUpdate;

    protected readonly assetCompletions = rxResource({
        stream: () => this.assetsService.getCompletions(this.appsState.appName),
    });

    protected readonly editForm = new EditAssetScriptsForm();

    constructor() {
        effect(() => {
            this.editForm.load(this.assetScriptsState.scripts());
        });

        effect(() => {
            this.editForm.setEnabled(this.isEditable());
        });

        this.assetScriptsState.load();
    }

    protected reload() {
        this.assetScriptsState.load(true);
    }

    protected selectField(field: string) {
        this.assetScript.set(field);
    }

    protected saveScripts() {
        if (!this.isEditable()) {
            return;
        }

        this.editForm.submit(value => {
            return this.assetScriptsState.update(value);
        });
    }
}
