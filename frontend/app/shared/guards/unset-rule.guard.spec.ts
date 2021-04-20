/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { RulesState } from '@app/shared/internal';
import { of } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';
import { UnsetRuleGuard } from './unset-rule.guard';

describe('UnsetRuleGuard', () => {
    let rulesState: IMock<RulesState>;
    let ruleGuard: UnsetRuleGuard;

    beforeEach(() => {
        rulesState = Mock.ofType<RulesState>();
        ruleGuard = new UnsetRuleGuard(rulesState.object);
    });

    it('should unset rule', () => {
        rulesState.setup(x => x.select(null))
            .returns(() => of(null));

        let result: boolean;

        ruleGuard.canActivate().subscribe(x => {
            result = x;
        }).unsubscribe();

        expect(result!).toBeTruthy();

        rulesState.verify(x => x.select(null), Times.once());
    });
});