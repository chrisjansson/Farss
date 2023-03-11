import { Union, Record } from "../Farss.Client/src/fable_modules/fable-library.3.7.18/Types.js";
import { bool_type, int32_type, option_type, tuple_type, array_type, uint8_type, union_type, class_type, record_type, string_type } from "../Farss.Client/src/fable_modules/fable-library.3.7.18/Reflection.js";

export class PreviewSubscribeToFeedQueryDto extends Record {
    constructor(Url) {
        super();
        this.Url = Url;
    }
}

export function PreviewSubscribeToFeedQueryDto$reflection() {
    return record_type("Dto.PreviewSubscribeToFeedQueryDto", [], PreviewSubscribeToFeedQueryDto, () => [["Url", string_type]]);
}

export class FeedError extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["FetchError", "ParseError"];
    }
}

export function FeedError$reflection() {
    return union_type("Dto.FeedError", [], FeedError, () => [[["Item", class_type("System.Exception")]], [["Item", class_type("System.Exception")]]]);
}

export class PreviewSubscribeToFeedResponseDto extends Record {
    constructor(Title, Url, Type, Icon, Protocol) {
        super();
        this.Title = Title;
        this.Url = Url;
        this.Type = Type;
        this.Icon = Icon;
        this.Protocol = Protocol;
    }
}

export function PreviewSubscribeToFeedResponseDto$reflection() {
    return record_type("Dto.PreviewSubscribeToFeedResponseDto", [], PreviewSubscribeToFeedResponseDto, () => [["Title", string_type], ["Url", string_type], ["Type", FeedType$reflection()], ["Icon", option_type(tuple_type(string_type, array_type(uint8_type)))], ["Protocol", Protocol$reflection()]]);
}

export class FeedType extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Atom", "Rss"];
    }
}

export function FeedType$reflection() {
    return union_type("Dto.FeedType", [], FeedType, () => [[], []]);
}

export class Protocol extends Union {
    constructor(tag, ...fields) {
        super();
        this.tag = (tag | 0);
        this.fields = fields;
    }
    cases() {
        return ["Http", "Https"];
    }
}

export function Protocol$reflection() {
    return union_type("Dto.Protocol", [], Protocol, () => [[], []]);
}

export class GetFileDto extends Record {
    constructor(Id) {
        super();
        this.Id = Id;
    }
}

export function GetFileDto$reflection() {
    return record_type("Dto.GetFileDto", [], GetFileDto, () => [["Id", class_type("System.Guid")]]);
}

export class FileDto extends Record {
    constructor(Id, FileName, Data) {
        super();
        this.Id = Id;
        this.FileName = FileName;
        this.Data = Data;
    }
}

export function FileDto$reflection() {
    return record_type("Dto.FileDto", [], FileDto, () => [["Id", class_type("System.Guid")], ["FileName", string_type], ["Data", array_type(uint8_type)]]);
}

export class SubscribeToFeedDto extends Record {
    constructor(Url, Title) {
        super();
        this.Url = Url;
        this.Title = Title;
    }
}

export function SubscribeToFeedDto$reflection() {
    return record_type("Dto.SubscribeToFeedDto", [], SubscribeToFeedDto, () => [["Url", string_type], ["Title", string_type]]);
}

export class SubscriptionDto extends Record {
    constructor(Id, Title, Url, Unread) {
        super();
        this.Id = Id;
        this.Title = Title;
        this.Url = Url;
        this.Unread = (Unread | 0);
    }
}

export function SubscriptionDto$reflection() {
    return record_type("Dto.SubscriptionDto", [], SubscriptionDto, () => [["Id", class_type("System.Guid")], ["Title", string_type], ["Url", string_type], ["Unread", int32_type]]);
}

export class ArticleDto extends Record {
    constructor(FeedId, Title, IsRead, PublishedAt, Content, Summary, Link) {
        super();
        this.FeedId = FeedId;
        this.Title = Title;
        this.IsRead = IsRead;
        this.PublishedAt = PublishedAt;
        this.Content = Content;
        this.Summary = Summary;
        this.Link = Link;
    }
}

export function ArticleDto$reflection() {
    return record_type("Dto.ArticleDto", [], ArticleDto, () => [["FeedId", class_type("System.Guid")], ["Title", string_type], ["IsRead", bool_type], ["PublishedAt", class_type("System.DateTimeOffset")], ["Content", string_type], ["Summary", option_type(string_type)], ["Link", string_type]]);
}

export class DeleteSubscriptionDto extends Record {
    constructor(Id) {
        super();
        this.Id = Id;
    }
}

export function DeleteSubscriptionDto$reflection() {
    return record_type("Dto.DeleteSubscriptionDto", [], DeleteSubscriptionDto, () => [["Id", option_type(class_type("System.Guid"))]]);
}

export class SetArticleReadStatusDto extends Record {
    constructor(ArticleId, SetIsReadTo) {
        super();
        this.ArticleId = ArticleId;
        this.SetIsReadTo = SetIsReadTo;
    }
}

export function SetArticleReadStatusDto$reflection() {
    return record_type("Dto.SetArticleReadStatusDto", [], SetArticleReadStatusDto, () => [["ArticleId", option_type(class_type("System.Guid"))], ["SetIsReadTo", option_type(bool_type)]]);
}

