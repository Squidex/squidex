/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Response, ResponseOptions } from '@angular/http';
import { Observable } from 'rxjs';
import { IMock, Mock, Times } from 'typemoq';

import {
    AppPlansDto,
    ApiUrlConfig,
    AuthService,
    ChangePlanDto,
    PlanDto,
    PlansService,
    Version
} from './../';

describe('PlansService', () => {
    let authService: IMock<AuthService>;
    let plansService: PlansService;
    let version = new Version('1');

    beforeEach(() => {
        authService = Mock.ofType(AuthService);
        plansService = new PlansService(authService.object, new ApiUrlConfig('http://service/p/'));
    });

    it('should make get request to get app plans', () => {
        authService.setup(x => x.authGet('http://service/p/api/apps/my-app/plans', version))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions({
                        body: {
                            currentPlanId: '123',
                            hasConfigured: true,
                            hasPortal: true,
                            planOwner: '456',
                            plans: [{
                                id: 'free',
                                name: 'Free',
                                costs: '14 €',
                                maxApiCalls: 1000,
                                maxAssetSize: 1500,
                                maxContributors: 2500
                            }, {
                                id: 'prof',
                                name: 'Prof',
                                costs: '18 €',
                                maxApiCalls: 4000,
                                maxAssetSize: 5500,
                                maxContributors: 6500
                            }]
                        }
                    })
                )
            ))
            .verifiable(Times.once());

        let plans: AppPlansDto | null = null;

        plansService.getPlans('my-app', version).subscribe(result => {
            plans = result;
        }).unsubscribe();

        expect(plans).toEqual(
            new AppPlansDto(
                '123',
                '456',
                true,
                true,
                [
                    new PlanDto('free', 'Free', '14 €', 1000, 1500, 2500),
                    new PlanDto('prof', 'Prof', '18 €', 4000, 5500, 6500)
                ]
            ));

        authService.verifyAll();
    });

    it('should make put request to change plan', () => {
        const dto = new ChangePlanDto('enterprise');

        authService.setup(x => x.authPut('http://service/p/api/apps/my-app/plan', dto, version))
            .returns(() => Observable.of(
                new Response(
                    new ResponseOptions()
                )
            ))
            .verifiable(Times.once());

        plansService.putPlan('my-app', dto, version);

        authService.verifyAll();
    });
});