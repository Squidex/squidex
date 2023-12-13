/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { NgFor, NgIf } from '@angular/common';
import { booleanAttribute, Component, ElementRef, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { delay } from 'rxjs/operators';
import { FocusOnInitDirective, ModalDialogComponent, ResizedDirective, ScrollActiveDirective, TooltipDirective, TranslatePipe } from '@app/framework';
import { AppsState, AuthService, StatefulComponent, TranslationsService } from '@app/shared/internal';
import { UserIdPicturePipe } from './pipes';

interface State {
    // True, when running
    isRunning: boolean;

    // The questions.
    chatQuestion: string;

    // The answers.
    chatTalk: ReadonlyArray<{ text: string; type: 'user' | 'bot' | 'system' }>;
}

@Component({
    standalone: true,
    selector: 'sqx-chat-dialog',
    styleUrls: ['./chat-dialog.component.scss'],
    templateUrl: './chat-dialog.component.html',
    imports: [
        FocusOnInitDirective,
        FormsModule,
        ModalDialogComponent,
        NgFor,
        NgIf,
        ResizedDirective,
        ScrollActiveDirective,
        TooltipDirective,
        TranslatePipe,
        UserIdPicturePipe,
    ],
})
export class ChatDialogComponent extends StatefulComponent<State> {
    @Output()
    public textSelect = new EventEmitter<string | undefined | null>();

    @Input({ required: true, transform: booleanAttribute })
    public showFormatHint = false;

    @ViewChild('input', { static: false })
    public input!: ElementRef<HTMLInputElement>;

    public user = this.authService.user!;

    constructor(
        private readonly appsState: AppsState,
        private readonly authService: AuthService,
        private readonly translator: TranslationsService,
    ) {
        super({
            isRunning: false,
            chatQuestion: '',
            chatTalk: [],
        });
    }

    public setQuestion(chatQuestion: string) {
        this.next({ chatQuestion });
    }

    public ask() {
        const prompt = this.snapshot.chatQuestion;

        if (!prompt || prompt.length === 0) {
            return;
        }

        this.next(s => ({
            ...s,
            chatQuestion: '',
            chatTalk: [
                ...s.chatTalk,
                { text: prompt, type: 'user' },
            ],
            isRunning: true,
        }));

        this.translator.ask(this.appsState.appName, { prompt }).pipe(delay(500))
            .subscribe({
                next: chatAnswers => {
                    if (chatAnswers.length === 0) {
                        this.next(s => ({
                            ...s,
                            chatQuestion: '',
                            chatTalk: [
                                ...s.chatTalk,
                                { text: 'i18n:chat.answersEmpty', type: 'system' },
                            ],
                            isRunning: true,
                        }));
                    } else {
                        this.next(s => ({
                            ...s,
                            chatTalk: [
                                ...s.chatTalk,
                                ...chatAnswers.map(text => ({ text, type: 'bot' } as any)),
                            ],
                            isRunning: false,
                        }));
                    }

                    setTimeout(() => {
                        this.input.nativeElement.focus();
                    }, 100);
                },
                error: () => {
                    this.next({ isRunning: false });
                },
                complete: () => {
                    this.next({ isRunning: false });
                },
            });
    }
}
