module App

open System
open Elmish
open Elmish.React
open Dto
open Html
open Model

let init(): Model * Cmd<Msg> = 
    let cmd = GuiCmd.loadSubsAndArticles
    Loading, cmd

let update (msg:Msg) (model:Model) =
    match msg with
    | Loaded (subs, articles) -> Model.Loaded { Subscriptions = subs; Articles = articles; SubInput = "" }, Cmd.none
    | LoadingError _ -> model, (GuiCmd.alert "Datta loading error hurr durr")
    | DeleteSubscription id -> 
        let cmd = GuiCmd.deleteSubscription id
        Loading, cmd
    | SubscriptionDeleted ->
        init()
    | SubscriptionDeleteFailed _ -> model, (GuiCmd.alert "Subscription delete failed")
    | Poll -> model, GuiCmd.poll
    | Reload -> Loading, GuiCmd.loadSubsAndArticles
    | OnChangeSub str -> 
        match model with 
        | Model.Loaded l ->
            Model.Loaded ({ l with SubInput = str }), Cmd.none
        | _ -> model, Cmd.none

let renderLoading () = 
    div [] [ str "Loading..."  ]

let renderLoaded (model: (Dto.SubscriptionDto list * Dto.ArticleDto list * string)) =
    let subscriptions, articles, inp = model
    
    let renderSubscription (subscription: SubscriptionDto) =
        div [] [
            str subscription.Url
            input [ _type "button"; value "x"; onClick (DeleteSubscription subscription.Id)  ] 
        ]
        
    let renderArticle (article: ArticleDto) =
        div [] [
            str article.Title
        ]
        
    div [] [
        div [] [
            input [ _type "button"; value "Poll"; onClick Poll ]
        ]
        div [] [
            h1 [] [str "Subscriptions"]
            div [] [
                str "Add"
                input [ onInput OnChangeSub; value inp ]
            ]
            fragment () [
                yield! subscriptions |> List.map renderSubscription
            ]
        ] 
        div [] [
            h1 [] [str "Articles"]
            fragment () [
                yield! articles |> List.map renderArticle
            ]
        ]
    ]

//    <nav class="navbar" role="navigation" aria-label="main navigation">

//  <div id="navbarBasicExample" class="navbar-menu">
//    <div class="navbar-start">

//      <a class="navbar-item">
//        Documentation
//      </a>

//      <div class="navbar-item has-dropdown is-hoverable">
//        <a class="navbar-link">
//          More
//        </a>

//        <div class="navbar-dropdown">
//          <a class="navbar-item">
//            About
//          </a>
//          <a class="navbar-item">
//            Jobs
//          </a>
//          <a class="navbar-item">
//            Contact
//          </a>
//          <hr class="navbar-divider">
//          <a class="navbar-item">
//            Report an issue
//          </a>
//        </div>
//      </div>
//    </div>

//    <div class="navbar-end">
//      <div class="navbar-item">
//        <div class="buttons">
//          <a class="button is-primary">
//            <strong>Sign up</strong>
//          </a>
//          <a class="button is-light">
//            Log in
//          </a>
//        </div>
//      </div>
//    </div>


//  </div>
//</nav>

module Nav =    
    open Fulma
    open Fable.Helpers.React

    module P = Fable.Helpers.React.Props
    module R = Fable.Helpers.React

    let nav (isOpen: bool) dispatch =
        let brandLogo _ =
            Navbar.Item.a [ Navbar.Item.Option.Props [(P.Href "https://bulma.io")] ] [
                    R.img [ P.Src "https://bulma.io/images/bulma-logo.png"; P.Style [ P.CSSProp.Width "112px"; P.CSSProp.Height "28px"] ]
            ]

        let burger _ =
            let classes = R.classList [
                "is-active", isOpen
                "navbar-burger", true
            ]

            let click = P.OnClick (fun _ -> dispatch ())

            R.a [ classes; click ] [
                R.span [] [] 
                R.span [] [] 
                R.span [] [] 
            ]

        let classes = R.classList [
            "is-active", isOpen
        ]

        Navbar.navbar [] [
            Navbar.Brand.div [] [
                brandLogo ()
                burger ()
            ]

            Navbar.menu [ 
                if isOpen then
                    yield Navbar.Menu.Option.CustomClass "is-active"
            ] [
                Navbar.Start.div [] [
                    Navbar.Item.a [] [ unbox "Home" ]
                ]
            ]
        ]
    
    let NavComp _ =
        let init _ = false
        let update () state = not state
        let view (m: ReactiveComponents.Model<_, bool>) d = nav m.state d
        Fable.Helpers.React.reactiveCom init update view "" () []


let view (model:Model) dispatch =
    Nav.NavComp ()
    //match model with
    //| Loading -> Html.run (renderLoading ()) dispatch
    //| Model.Loaded { Subscriptions = subs; Articles = articles; SubInput = s } -> Html.run (renderLoaded (subs, articles, s)) dispatch

Program.mkProgram init update view
    |> Program.withReact "elmish-app"
    |> Program.withConsoleTrace
    |> Program.run
