/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Observable, shareReplay } from 'rxjs';
import { AppsState, AssetScriptsState, AssetsService, CodeEditorComponent, EditAssetScriptsForm, KeysPipe, LayoutComponent, ScriptCompletions, ShortcutDirective, SidebarMenuDirective, TitleComponent, TooltipDirective, TourStepDirective, TranslatePipe } from '@app/shared';
import { ScriptNamePipe } from '@app/shared/components/pipes';

@Component({
    selector: 'sqx-asset-scripts-page',
    styleUrls: ['./asset-scripts-page.component.scss'],
    templateUrl: './asset-scripts-page.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        CodeEditorComponent,
        FormsModule,
        KeysPipe,
        LayoutComponent,
        ReactiveFormsModule,
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
    private readonly assetScriptsState = inject(AssetScriptsState);

    protected readonly assetScript = signal('query');
    protected readonly assetCompletions: Observable<ScriptCompletions>;

    protected readonly editForm = new EditAssetScriptsForm();

    protected readonly isEditable = this.assetScriptsState.canUpdate;

    constructor() {
        const appsState = inject(AppsState);
        const assetsService = inject(AssetsService);

        this.assetCompletions = assetsService.getCompletions(appsState.appName).pipe(shareReplay(1));

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

        const value = this.editForm.submit();
        if (!value) {
            return;
        }

        this.assetScriptsState.update(value)
            .subscribe({
                next: () => {
                    this.editForm.submitCompleted({ noReset: true });
                },
                error: error => {
                    this.editForm.submitFailed(error);
                },
            });
    }
}
