﻿// Copyright (c) CBC/Radio-Canada. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for more information.

namespace LinkIt.TestHelpers
{
    public class Media:IPolymorphicModel {
        public int Id { get; set; }
        public string Title { get; set; }
        public string SummaryImageId { get; set; }
    }
}