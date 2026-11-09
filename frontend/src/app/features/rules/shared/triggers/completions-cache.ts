/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { inject, Service } from '@angular/core';
import { EMPTY, map, Observable, shareReplay } from 'rxjs';
import { SchemasService, SchemasState, ScriptCompletions } from '@app/shared';

@Service({ autoProvided: false })
export class CompletionsCache {
    private readonly schemasService = inject(SchemasService);
    private readonly schemasState = inject(SchemasState);
    private readonly cache: { [schema: string]: Observable<ScriptCompletions> } = {};

    public getCompletions(schema: string, skipFunctions: boolean) {
        if (!schema) {
            return EMPTY;
        }

        let result = this.cache[schema];

        if (!result) {
            result = this.schemasService.getContentTriggerCompletion(this.schemasState.appName, schema).pipe(shareReplay(1));
            this.cache[schema] = result;
        }

        if (skipFunctions) {
            result = result.pipe(map(x => x.filter(c => c.type !== 'Function')));
        }

        return result;
    }
}